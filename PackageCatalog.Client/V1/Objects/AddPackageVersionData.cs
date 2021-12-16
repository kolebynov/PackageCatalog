namespace PackageCatalog.Client.V1.Objects;

public record AddPackageVersionData(string PackageId, Version Version, IReadOnlyDictionary<string, string>? AdditionalData,
	Stream PackageContent)
{
	public string PackageId { get; } = !string.IsNullOrEmpty(PackageId)
		? PackageId : throw new ArgumentException("Value cannot be null or empty.", nameof(PackageId));

	public Version Version { get; } = Version ?? throw new ArgumentNullException(nameof(Version));

	public Stream PackageContent { get; } = PackageContent ?? throw new ArgumentNullException(nameof(PackageContent));
}