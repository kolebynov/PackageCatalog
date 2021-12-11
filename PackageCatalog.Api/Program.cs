using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PackageCatalog.Api;
using PackageCatalog.Api.Infrastructure;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Internal;
using PackageCatalog.Core.Extensions;
using PackageCatalog.EfRepository.Extensions;
using PackageCatalog.FileSystemStorage.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(opt =>
{
	opt.IncludeExceptionDetails = (_, _) => false;
	opt.Map<Exception>((context, e) =>
	{
		var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
		return factory.CreateProblemDetails(context, StatusCodes.Status500InternalServerError, detail: e.Message);
	});
});
builder.Services.AddControllers();
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
				new OpenApiInfo()
				{
					Title = $"Sample API {description.ApiVersion}",
					Version = description.ApiVersion.ToString(),
				});
		}
	});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCorePackageServices();
builder.Services.AddFileSystemPackageStorage();
builder.Services.AddEfPackageRepository(opt =>
{
	var connectionStringBuilder = new SqliteConnectionStringBuilder(builder.Configuration["connectionString"])
	{
		ForeignKeys = true,
	};
	opt.UseSqlite(connectionStringBuilder.ToString());
});

builder.Services.AddScoped<DatabaseMaintenance>();

builder.Services.AddSingleton<ISkipTokenGenerator, SkipTokenGenerator>();

var app = builder.Build();

app.UseProblemDetails();

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

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
await scope.ServiceProvider.GetRequiredService<DatabaseMaintenance>().Apply();

await app.RunAsync();