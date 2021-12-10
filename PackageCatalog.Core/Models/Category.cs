namespace PackageCatalog.Core.Models;

public class Category
{
	public string Id { get; }

	public string? DisplayName { get; }

	public Category(string id, string? displayName)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(id));
		}

		Id = id;
		DisplayName = displayName;
	}

	public override string ToString() => DisplayName ?? Id;
}