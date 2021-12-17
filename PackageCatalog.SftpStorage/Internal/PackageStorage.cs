using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.SftpStorage.Configuration;
using PackageCatalog.SftpStorage.Internal.Interfaces;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace PackageCatalog.SftpStorage.Internal;

internal class PackageStorage : IPackageStorage
{
	private readonly ISftpClientEx sftpClient;
	private readonly ILogger<PackageStorage> logger;
	private readonly SftpStorageSettings settings;

	public PackageStorage(ISftpClientEx sftpClient, ILogger<PackageStorage> logger, IOptions<SftpStorageSettings> settings)
	{
		this.sftpClient = sftpClient ?? throw new ArgumentNullException(nameof(sftpClient));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
	}

	public Task<Stream> GetPackageContent(string path, CancellationToken cancellationToken)
	{
		logger.LogInformation("Getting package content. [Path: {Path}]", path);

		IDisposable? activeConnection = null;
		try
		{
			activeConnection = sftpClient.Connect();
			return Task.FromResult<Stream>(
				new SftpAutoDisconnectFileStream(sftpClient.OpenRead(GetFullPath(path)), activeConnection));
		}
		catch (SftpPathNotFoundException e)
		{
			activeConnection?.Dispose();
			throw new PackageCatalogException("Package content was not found in the storage", e);
		}
		catch
		{
			activeConnection?.Dispose();
			throw;
		}
	}

	public async Task StorePackageContent(string path, Stream packageData, CancellationToken cancellationToken)
	{
		logger.LogInformation("Storing package content. [Path: {Path}]", path);

		var fullPath = GetFullPath(path);

		IAsyncResult BeginUploadFile(AsyncCallback callback, object? state) =>
			sftpClient.BeginUploadFile(packageData, fullPath, true, callback, state);

		try
		{
			using var connection = sftpClient.Connect();
			if (!sftpClient.Exists(Path.GetDirectoryName(fullPath)))
			{
				sftpClient.CreateDirectory(Path.GetDirectoryName(fullPath));
			}

			await Task.Factory.FromAsync(BeginUploadFile, sftpClient.EndUploadFile, null, TaskCreationOptions.None);
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

	private class SftpAutoDisconnectFileStream : Stream
	{
		private readonly SftpFileStream innerStream;
		private readonly IDisposable activeConnection;

		public override bool CanRead => innerStream.CanRead;

		public override bool CanSeek => innerStream.CanSeek;

		public override bool CanWrite => innerStream.CanWrite;

		public override long Length => innerStream.Length;

		public override long Position
		{
			get => innerStream.Position;
			set => innerStream.Position = value;
		}

		public SftpAutoDisconnectFileStream(SftpFileStream innerStream, IDisposable activeConnection)
		{
			this.innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
			this.activeConnection = activeConnection ?? throw new ArgumentNullException(nameof(activeConnection));
		}

		public override void Flush() => innerStream.Flush();

		public override int ReadByte() => innerStream.ReadByte();

		public override int Read(byte[] buffer, int offset, int count) => innerStream.Read(buffer, offset, count);

		public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);

		public override void SetLength(long value) => innerStream.SetLength(value);

		public override void WriteByte(byte value) => innerStream.WriteByte(value);

		public override void Write(byte[] buffer, int offset, int count) => innerStream.Write(buffer, offset, count);

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				innerStream.Dispose();
				activeConnection.Dispose();
			}
		}
	}
}