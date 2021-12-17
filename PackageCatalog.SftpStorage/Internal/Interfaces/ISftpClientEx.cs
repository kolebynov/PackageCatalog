using Renci.SshNet;

namespace PackageCatalog.SftpStorage.Internal.Interfaces;

internal interface ISftpClientEx : ISftpClient
{
	IDisposable Connect();
}