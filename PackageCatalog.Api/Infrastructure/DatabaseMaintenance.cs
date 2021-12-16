using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PackageCatalog.Shared.Interfaces;

namespace PackageCatalog.Api.Infrastructure;

public class DatabaseMaintenance
{
	private readonly ILogger<DatabaseMaintenance> logger;
	private readonly DbContext context;
	private readonly IFileSystemAdapter fileSystemAdapter;

	public DatabaseMaintenance(ILogger<DatabaseMaintenance> logger, DbContext context,
		IFileSystemAdapter fileSystemAdapter)
	{
		this.logger = logger;
		this.context = context;
		this.fileSystemAdapter = fileSystemAdapter;
	}

	public async Task Apply()
	{
		EnsureDirectoryExists();
		await Migrate();
		await Vacuum();
	}

	private async Task Migrate()
	{
		logger.LogInformation("Migrating the database...");
		await context.Database.MigrateAsync();
		logger.LogInformation("The database schema is up to date");
	}

	private async Task Vacuum()
	{
		var dbSizeBefore = await GetDbSizeInKb();

		logger.LogInformation("Vacuuming the database...");
		await context.Database.ExecuteSqlRawAsync("VACUUM");

		var dbSizeAfter = await GetDbSizeInKb();

		logger.LogInformation(
			"The database is vacuumed. Was: {OldSize:0.0}Kb, now: {NewSize:0.0}Kb, saved: {Saved:0.0}Kb",
			dbSizeBefore, dbSizeAfter, dbSizeBefore - dbSizeAfter);
	}

	private async Task<double> GetDbSizeInKb()
	{
		var pageSize = await context.Database.ExecuteSqlRawAsync("PRAGMA page_size");
		var pageCount = await context.Database.ExecuteSqlRawAsync("PRAGMA page_count");
		return pageSize * pageCount / 1024.0;
	}

	private void EnsureDirectoryExists()
	{
		var dbConnection = context.Database.GetDbConnection();
		var stringBuilder = new SqliteConnectionStringBuilder(dbConnection.ConnectionString);
		var databaseDirectory = Path.GetDirectoryName(stringBuilder.DataSource);
		if (databaseDirectory == null)
		{
			throw new InvalidOperationException("Database directory is null");
		}

		fileSystemAdapter.EnsureDirectoryExists(databaseDirectory);
	}
}