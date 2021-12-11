namespace PackageCatalog.Contracts.V1;

public class AddPackageVersionRequestV1
{
	public Version Version { get; init; } = null!;

	public IReadOnlyDictionary<string, string>? AdditionalData { get; init; }

	public string UploadedContentTicket { get; init; } = null!;
}