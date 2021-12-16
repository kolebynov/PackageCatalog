using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.FileSystemStorage.Configuration;
using PackageCatalog.Shared.Interfaces;

namespace PackageCatalog.FileSystemStorage.Internal;

internal class PackageStorage : IPackageStorage
{
	private readonly IFileProvider fileProvider;
	private readonly IFileSystemAdapter fileSystemAdapter;
	private readonly FileSystemStorageSettings settings;
	private readonly ILogger<PackageStorage> logger;

	public PackageStorage(IFileProvider fileProvider, IOptions<FileSystemStorageSettings> settings,
		ILogger<PackageStorage> logger, IFileSystemAdapter fileSystemAdapter)
	{
		this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.fileSystemAdapter = fileSystemAdapter ?? throw new ArgumentNullException(nameof(fileSystemAdapter));
		this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
	}

	public Task<Stream> GetPackageContent(string path, CancellationToken cancellationToken)
	{
		logger.LogInformation("Getting package content. [Path: {Path}]", path);

		var fileInfo = fileProvider.GetFileInfo(path);
		if (!fileInfo.Exists || fileInfo.IsDirectory)
		{
			throw new NotFoundPackageCatalogException("Package content not found");
		}

		return Task.FromResult(fileInfo.CreateReadStream());
	}

	public async Task StorePackageContent(string path, Stream packageData, CancellationToken cancellationToken)
	{
		logger.LogInformation("Storing package content to the path {Path}", path);

		var fullPath = fileProvider.GetFileInfo(path).PhysicalPath;
		if (string.IsNullOrEmpty(fullPath))
		{
			throw new ArgumentException("Incorrect path provided", nameof(path));
		}

		try
		{
			fileSystemAdapter.EnsureDirectoryExists(Path.GetDirectoryName(fullPath)!);
			using var fileStream = fileSystemAdapter.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read);
			await packageData.CopyToAsync(fileStream, cancellationToken);
		}
		catch (Exception e)
		{
			throw new PackageCatalogException("Failed to save package content", e);
		}
	}
}