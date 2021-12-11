using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Models;

public class Package
{
	public StringId Id { get; }

	public string? DisplayName { get; }

	public StringId CategoryId { get; }

	public Package(StringId id, string? displayName, StringId categoryId)
	{
		Id = id ?? throw new ArgumentNullException(nameof(id));
		DisplayName = displayName;
		CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
	}

	public override string ToString() => DisplayName ?? Id.Value;
}