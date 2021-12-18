using Renci.SshNet;

namespace PackageCatalog.SftpStorage.Internal.Interfaces;

internal interface ISftpClientAccessor
{
	ISftpClient SftpClient { get; }
}