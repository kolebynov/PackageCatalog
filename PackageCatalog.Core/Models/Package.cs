namespace PackageCatalog.Core.Models;

public class Package
{
	public string Id { get; }

	public string? DisplayName { get; }

	public string CategoryId { get; }

	public Package(string id, string? displayName, string categoryId)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(id));
		}

		if (string.IsNullOrEmpty(categoryId))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(categoryId));
		}

		Id = id;
		DisplayName = displayName;
		CategoryId = categoryId;
	}

	public override string ToString() => DisplayName ?? Id;
}