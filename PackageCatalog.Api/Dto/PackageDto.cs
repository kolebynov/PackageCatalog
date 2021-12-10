using System.Text.Json.Serialization;

namespace PackageCatalog.Api.Dto;

public class PackageDto
{
	public string Id { get; init; } = null!;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? DisplayName { get; init; }
}