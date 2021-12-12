namespace PackageCatalog.Core.Objects;

public record AddPackageData(StringId PackageId, string? DisplayName, StringId CategoryId)
{
	public StringId PackageId { get; } = PackageId ?? throw new ArgumentNullException(nameof(PackageId));

	public StringId CategoryId { get; } = CategoryId ?? throw new ArgumentNullException(nameof(CategoryId));
}