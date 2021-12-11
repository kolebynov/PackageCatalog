namespace PackageCatalog.Api.Configuration;

public class TempContentStorageSettings
{
	public string Path { get; set; } = "temp";

	public int FoldersCount { get; set; } = 10;

	public TimeSpan TempContentLifetime { get; set; } = TimeSpan.FromHours(1);
}