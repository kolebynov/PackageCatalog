namespace PackageCatalog.Core.Objects;

public class AddPackageVersionData
{
	public StringId PackageId { get; }

	public Version Version { get; }

	public IReadOnlyDictionary<string, string>? AdditionalData { get; }

	public Stream Content { get; }

	public AddPackageVersionData(StringId packageId, Version version,
		IReadOnlyDictionary<string, string>? additionalData, Stream content)
	{
		PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
		Version = version ?? throw new ArgumentNullException(nameof(version));
		Content = content ?? throw new ArgumentNullException(nameof(content));
		AdditionalData = additionalData;
	}
}