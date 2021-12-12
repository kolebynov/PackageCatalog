using System.ComponentModel.DataAnnotations;

namespace PackageCatalog.Contracts.V1;

public class AddCategoryRequestV1
{
	[Required]
	[RegularExpression(Constants.IdRegex)]
	public string Id { get; init; } = null!;

	public string? DisplayName { get; init; }
}