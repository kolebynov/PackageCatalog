using Microsoft.Extensions.Logging;
using PackageCatalog.SftpStorage.Internal.Interfaces;
using Renci.SshNet;

namespace PackageCatalog.SftpStorage.Internal;

internal class SftpClientWrapper
{
	private readonly SftpClient sftpClient;
	private readonly object lockObject = new();
	private readonly ILogger<SftpClientWrapper> logger;
	private int activeConnections;

	public SftpClientWrapper(SftpClient sftpClient, ILogger<SftpClientWrapper> logger)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.sftpClient = sftpClient ?? throw new ArgumentNullException(nameof(sftpClient));
	}

	public ISftpClientAccessor CreateAccessor() => new SftpClientAccessor(this);

	private void Connect()
	{
		lock (lockObject)
		{
			activeConnections++;
			logger.LogDebug(
				"Incrementing active SFTP connections. [Active connections: {ActiveConnection}]", activeConnections);

			if (sftpClient.IsConnected)
			{
				return;
			}

			logger.LogInformation("Connecting to SFTP server");
			sftpClient.Connect();
		}
	}

	private void Disconnect()
	{
		lock (lockObject)
		{
			activeConnections--;
			logger.LogDebug(
				"Decrementing active SFTP connections. [Active connections: {ActiveConnection}]", activeConnections);

			switch (activeConnections)
			{
				case 0:
					logger.LogInformation("Disconnecting from SFTP server");
					sftpClient.Disconnect();
					break;
				case < 0:
					activeConnections = 0;
					break;
			}
		}
	}

	private sealed class SftpClientAccessor : ISftpClientAccessor, IDisposable
	{
		private readonly Lazy<SftpClientWrapper> sftpClientWrapperLazy;

		public ISftpClient SftpClient => sftpClientWrapperLazy.Value.sftpClient;

		public SftpClientAccessor(SftpClientWrapper sftpClientWrapper)
		{
			sftpClientWrapperLazy = new Lazy<SftpClientWrapper>(() =>
			{
				sftpClientWrapper.Connect();
				return sftpClientWrapper;
			});
		}

		public void Dispose()
		{
			if (sftpClientWrapperLazy.IsValueCreated)
			{
				sftpClientWrapperLazy.Value.Disconnect();
			}
		}
	}
}