using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PackageCatalog.Api;
using PackageCatalog.Api.Configuration;
using PackageCatalog.Api.Exceptions;
using PackageCatalog.Api.Infrastructure;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Internal;
using PackageCatalog.Core;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Extensions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Objects;
using PackageCatalog.EfRepository.Extensions;
using PackageCatalog.SftpStorage.Extensions;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Host
	.UseSerilog((context, loggerConfiguration) =>
		loggerConfiguration
			.ReadFrom.Configuration(context.Configuration)
			.Destructure.ByTransforming<AddPackageVersionData>(
				x => new { x.PackageId, x.Version, x.AdditionalData, PackageSize = x.Content.Length })
			.Destructure.AsScalar<StringId>()
			.Destructure.AsScalar<Version>()
			.Enrich.FromLogContext());

builder.Services.AddProblemDetails(opt =>
{
	opt.ShouldLogUnhandledException = (_, exception, _) => exception switch
	{
		NotFoundPackageCatalogException => false,
		ForbiddenPackageCatalogException => false,
		_ => true,
	};
	opt.Map<NotFoundPackageCatalogException>((context, e) =>
	{
		var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
		return factory.CreateProblemDetails(context, StatusCodes.Status404NotFound, detail: e.Message);
	});
	opt.Map<ForbiddenPackageCatalogException>((context, e) =>
	{
		var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
		return factory.CreateProblemDetails(context, StatusCodes.Status403Forbidden, detail: e.Message);
	});
	opt.Map<Exception>((context, e) =>
	{
		var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
		return factory.CreateProblemDetails(context, StatusCodes.Status500InternalServerError, detail: e.Message);
	});
});
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opt =>
	{
		opt.RequireHttpsMetadata = true;
		opt.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = JwtOptions.Issuer,
			ValidateAudience = true,
			ValidAudience = JwtOptions.Audience,
			ValidateLifetime = true,
			IssuerSigningKey = JwtOptions.SymmetricSecurityKey,
			ValidateIssuerSigningKey = true,
		};
	});
builder.Services.AddAuthorization();
builder.Services.AddApiVersioning(opt =>
{
	opt.ReportApiVersions = true;
	opt.ApiVersionReader = new QueryStringApiVersionReader(Constants.ApiVersionParameterName);
	opt.DefaultApiVersion = new ApiVersion(1, 0);
	opt.AssumeDefaultVersionWhenUnspecified = true;
});
builder.Services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
builder.Services.AddOptions<SwaggerGenOptions>()
	.Configure((SwaggerGenOptions opt, IApiVersionDescriptionProvider provider) =>
	{
		foreach (var description in provider.ApiVersionDescriptions)
		{
			opt.SwaggerDoc(
				description.GroupName,
				new OpenApiInfo
				{
					Title = $"Sample API {description.ApiVersion}",
					Version = description.ApiVersion.ToString(),
				});
		}
	});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
	opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Please insert JWT with Bearer into field",
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey,
	});
	opt.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer",
				},
			},
			Array.Empty<string>()
		},
	});
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddCorePackageServices();
builder.Services.AddSftpStorage(opt => builder.Configuration.Bind("sftpStorage", opt));
builder.Services.AddEfPackageRepository(opt =>
{
	var connectionStringBuilder = new SqliteConnectionStringBuilder(builder.Configuration["connectionString"])
	{
		ForeignKeys = true,
	};
	opt.UseSqlite(connectionStringBuilder.ToString());
});

builder.Services.Configure<TempContentStorageSettings>(builder.Configuration.GetSection("tempContentStorage"));

builder.Services.AddScoped<DatabaseMaintenance>();
builder.Services.AddScoped<IScopeAccessor, ScopeAccessor>();
builder.Services.AddScoped<PackageCatalogService>();
builder.Services.Replace(ServiceDescriptor.Scoped<IPackageCatalogService>(
	sp => ActivatorUtilities.CreateInstance<ScopePackageCatalogService>(
		sp, sp.GetRequiredService<PackageCatalogService>())));

builder.Services.AddSingleton<ISkipTokenGenerator, SkipTokenGenerator>();
builder.Services.AddSingleton<ITempContentStorage, TempContentStorage>();
builder.Services.AddSingleton<IScopeParser, ScopeParser>();

builder.Services.AddHostedService(sp => (TempContentStorage)sp.GetRequiredService<ITempContentStorage>());

var app = builder.Build();

app.UseProblemDetails();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(opt =>
	{
		var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
		foreach (var description in provider.ApiVersionDescriptions)
		{
			opt.SwaggerEndpoint(
				$"/swagger/{description.GroupName}/swagger.json",
				description.GroupName.ToUpperInvariant());
		}
	});
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
await scope.ServiceProvider.GetRequiredService<DatabaseMaintenance>().Apply();

await app.RunAsync();