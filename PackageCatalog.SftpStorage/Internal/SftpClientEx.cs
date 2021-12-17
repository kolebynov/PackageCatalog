using Microsoft.Extensions.Logging;
using PackageCatalog.SftpStorage.Internal.Interfaces;
using Renci.SshNet;

namespace PackageCatalog.SftpStorage.Internal;

public class SftpClientEx : SftpClient, ISftpClientEx
{
	private readonly object lockObject = new();
	private readonly ActiveConnectionDisposable activeConnectionDisposable;
	private readonly ILogger<SftpClientEx> logger;
	private int activeConnections;

	public SftpClientEx(ConnectionInfo connectionInfo, ILogger<SftpClientEx> logger)
		: base(connectionInfo)
	{
		this.logger = logger;
		activeConnectionDisposable = new ActiveConnectionDisposable(this);
	}

	public SftpClientEx(string host, int port, string username, string password, ILogger<SftpClientEx> logger)
		: base(host, port, username, password)
	{
		this.logger = logger;
		activeConnectionDisposable = new ActiveConnectionDisposable(this);
	}

	public SftpClientEx(string host, string username, string password, ILogger<SftpClientEx> logger)
		: base(host, username, password)
	{
		this.logger = logger;
		activeConnectionDisposable = new ActiveConnectionDisposable(this);
	}

	public SftpClientEx(string host, int port, string username, ILogger<SftpClientEx> logger, params PrivateKeyFile[] keyFiles)
		: base(host, port, username, keyFiles)
	{
		this.logger = logger;
		activeConnectionDisposable = new ActiveConnectionDisposable(this);
	}

	public SftpClientEx(string host, string username, ILogger<SftpClientEx> logger, params PrivateKeyFile[] keyFiles)
		: base(host, username, keyFiles)
	{
		this.logger = logger;
		activeConnectionDisposable = new ActiveConnectionDisposable(this);
	}

	IDisposable ISftpClientEx.Connect()
	{
		lock (lockObject)
		{
			if (activeConnections == 0)
			{
				logger.LogInformation("Connecting to SFTP server");
				Connect();
			}

			logger.LogDebug("Incrementing active SFTP connections");
			activeConnections++;
		}

		return activeConnectionDisposable;
	}

	private void DisconnectInternal()
	{
		lock (lockObject)
		{
			logger.LogDebug("Decrementing active SFTP connections");
			activeConnections--;

			switch (activeConnections)
			{
				case 0:
					logger.LogInformation("Disconnecting from SFTP server");
					Disconnect();
					break;
				case < 0:
					activeConnections = 0;
					break;
			}
		}
	}

	private sealed class ActiveConnectionDisposable : IDisposable
	{
		private readonly SftpClientEx sftpClientEx;

		public ActiveConnectionDisposable(SftpClientEx sftpClientEx)
		{
			this.sftpClientEx = sftpClientEx;
		}

		public void Dispose() => sftpClientEx.DisconnectInternal();
	}
}