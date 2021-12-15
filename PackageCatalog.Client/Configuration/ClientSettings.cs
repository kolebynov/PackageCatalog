namespace PackageCatalog.Client.Configuration;

public class ClientSettings
{
	public Uri BaseUri { get; set; } = null!;

	public string AccessToken { get; set; } = null!;
}