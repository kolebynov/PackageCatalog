using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PackageCatalog.Api.Infrastructure;
using PackageCatalog.Core.Extensions;
using PackageCatalog.EfRepository.Extensions;
using PackageCatalog.FileSystemStorage.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
await scope.ServiceProvider.GetRequiredService<DatabaseMaintenance>().Apply();

await app.RunAsync();