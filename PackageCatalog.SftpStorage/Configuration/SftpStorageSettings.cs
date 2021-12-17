namespace PackageCatalog.SftpStorage.Configuration;

public class SftpStorageSettings
{
	public string Host { get; set; } = null!;

	public int Port { get; set; } = 22;

	public string BasePath { get; set; } = "/packages";

	public string UserName { get; set; } = null!;

	public string Password { get; set; } = null!;
}