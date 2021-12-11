namespace PackageCatalog.Contracts.V1;

public class PackageVersionV1
{
	public string PackageId { get; init; } = null!;

	public Version Version { get; init; } = null!;
}