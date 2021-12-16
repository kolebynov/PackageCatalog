using System.Text;
using Microsoft.Extensions.Options;
using PackageCatalog.Api.Configuration;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Shared.Interfaces;

namespace PackageCatalog.Api.Internal;

internal class TempContentStorage : BackgroundService, ITempContentStorage
{
	private readonly IFileSystemAdapter fileSystemAdapter;
	private readonly TempContentStorageSettings settings;
	private readonly ILogger<TempContentStorage> logger;
	private readonly Random random = new();

	private bool isFoldersInitialized;

	public TempContentStorage(IFileSystemAdapter fileSystemAdapter, IOptions<TempContentStorageSettings> settings,
		ILogger<TempContentStorage> logger)
	{
		this.fileSystemAdapter = fileSystemAdapter ?? throw new ArgumentNullException(nameof(fileSystemAdapter));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
	}

	public async Task<string> StoreTempContent(Stream stream, CancellationToken cancellationToken)
	{
		InitializeFolders();

		var localPath = Path.Combine($"{random.Next(0, settings.FoldersCount)}", Path.GetFileName(Path.GetRandomFileName()));
		var ticket = Convert.ToBase64String(Encoding.UTF8.GetBytes(localPath));
		logger.LogDebug("Storing temp content. [Ticket: {Ticket}]", ticket);

		using var fileStream = fileSystemAdapter.Open(Path.Combine(settings.Path, localPath), FileMode.Create,
			FileAccess.Write, FileShare.Read);
		await stream.CopyToAsync(fileStream, cancellationToken);

		logger.LogDebug("Temp content has been saved. [Ticket: {Ticket}][Size: {Size}]", ticket, fileStream.Length);

		return ticket;
	}

	public Task<Stream> GetTempContent(string ticket, CancellationToken cancellationToken)
	{
		logger.LogDebug("Getting temp content. [Ticket: {Ticket}]", ticket);

		var localPath = Encoding.UTF8.GetString(Convert.FromBase64String(ticket));
		var fullPath = Path.Combine(settings.Path, localPath);

		if (!fileSystemAdapter.FileExists(fullPath))
		{
			throw new ArgumentException("Invalid or expired ticket", nameof(ticket));
		}

		return Task.FromResult(fileSystemAdapter.OpenRead(fullPath));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		while (!stoppingToken.IsCancellationRequested)
		{
			if (!fileSystemAdapter.DirectoryExists(settings.Path))
			{
				continue;
			}

			var now = DateTimeOffset.UtcNow;
			var filesToRemove = fileSystemAdapter.EnumerateFiles(settings.Path, "*", SearchOption.AllDirectories)
				.Where(x => now - fileSystemAdapter.GetLastWriteTime(x) > settings.TempContentLifetime);
			foreach (var fileToRemove in filesToRemove)
			{
				try
				{
					fileSystemAdapter.DeleteFile(fileToRemove);
				}
				catch (Exception e)
				{
					logger.LogWarning(e, "Failed to delete temp content file {File}", filesToRemove);
				}
			}

			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
		}
	}

	private void InitializeFolders()
	{
		if (isFoldersInitialized)
		{
			return;
		}

		for (var i = 0; i < settings.FoldersCount; i++)
		{
			fileSystemAdapter.EnsureDirectoryExists(Path.Combine(settings.Path, $"{i}"));
		}

		isFoldersInitialized = true;
	}
}