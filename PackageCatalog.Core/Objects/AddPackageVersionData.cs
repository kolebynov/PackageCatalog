namespace PackageCatalog.Core.Objects;

public record AddPackageVersionData(StringId PackageId, Version Version,
	IReadOnlyDictionary<string, string>? AdditionalData, Stream Content)
{
	public StringId PackageId { get; } = PackageId ?? throw new ArgumentNullException(nameof(PackageId));

	public Version Version { get; } = Version ?? throw new ArgumentNullException(nameof(Version));

	public Stream Content { get; } = Content ?? throw new ArgumentNullException(nameof(Content));
}