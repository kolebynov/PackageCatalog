using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.SftpStorage.Configuration;
using PackageCatalog.SftpStorage.Internal.Interfaces;
using Renci.SshNet.Common;

namespace PackageCatalog.SftpStorage.Internal;

internal class PackageStorage : IPackageStorage
{
	private readonly ISftpClientAccessor sftpClientAccessor;
	private readonly ILogger<PackageStorage> logger;
	private readonly SftpStorageSettings settings;

	public PackageStorage(ISftpClientAccessor sftpClientAccessor, ILogger<PackageStorage> logger, IOptions<SftpStorageSettings> settings)
	{
		this.sftpClientAccessor = sftpClientAccessor ?? throw new ArgumentNullException(nameof(sftpClientAccessor));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
	}

	public Task<Stream> GetPackageContent(string path, CancellationToken cancellationToken)
	{
		logger.LogInformation("Getting package content. [Path: {Path}]", path);

		try
		{
			return Task.FromResult<Stream>(sftpClientAccessor.SftpClient.OpenRead(GetFullPath(path)));
		}
		catch (SftpPathNotFoundException e)
		{
			throw new PackageCatalogException("Package content was not found in the storage", e);
		}
	}

	public async Task StorePackageContent(string path, Stream packageData, CancellationToken cancellationToken)
	{
		logger.LogInformation("Storing package content. [Path: {Path}]", path);

		var fullPath = GetFullPath(path);

		IAsyncResult BeginUploadFile(AsyncCallback callback, object? state) =>
			sftpClientAccessor.SftpClient.BeginUploadFile(packageData, fullPath, true, callback, state);

		try
		{
			if (!sftpClientAccessor.SftpClient.Exists(Path.GetDirectoryName(fullPath)))
			{
				sftpClientAccessor.SftpClient.CreateDirectory(Path.GetDirectoryName(fullPath));
			}

			await Task.Factory.FromAsync(BeginUploadFile, sftpClientAccessor.SftpClient.EndUploadFile, null,
				TaskCreationOptions.None);
		}
		catch (Exception e)
		{
			throw new PackageCatalogException("Failed to upload package content", e);
		}
	}

	private string GetFullPath(string path)
	{
		var fullPath = Path.Combine(settings.BasePath, path.TrimStart('/')).Replace('\\', '/');
		if (!fullPath.StartsWith(settings.BasePath, StringComparison.Ordinal))
		{
			throw new ArgumentException("Invalid path provider");
		}

		return fullPath;
	}
}