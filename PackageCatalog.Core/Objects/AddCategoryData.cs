namespace PackageCatalog.Core.Objects;

public record AddCategoryData(StringId Id, string? DisplayName)
{
	public StringId Id { get; } = Id ?? throw new ArgumentNullException(nameof(Id));
}