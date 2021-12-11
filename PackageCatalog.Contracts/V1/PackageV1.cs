using System.Text.Json.Serialization;

namespace PackageCatalog.Contracts.V1;

public class PackageV1
{
	public string Id { get; init; } = null!;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? DisplayName { get; init; }
}