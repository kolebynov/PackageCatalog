using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PackageCatalog.Api.Dto;

public class CategoryDto
{
	public string Id { get; init; } = null!;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? DisplayName { get; init; }
}