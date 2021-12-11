using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Models;

public class Category
{
	public StringId Id { get; }

	public string? DisplayName { get; }

	public Category(StringId id, string? displayName)
	{
		Id = id ?? throw new ArgumentNullException(nameof(id));
		DisplayName = displayName;
	}

	public override string ToString() => DisplayName ?? Id.Value;
}