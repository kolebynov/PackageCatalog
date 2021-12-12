using System.ComponentModel.DataAnnotations;

namespace PackageCatalog.Contracts.V1;

public class AddPackageRequestV1
{
	[Required]
	[RegularExpression(Constants.IdRegex)]
	public string PackageId { get; init; } = null!;

	public string? DisplayName { get; init; }

	[Required]
	[RegularExpression(Constants.IdRegex)]
	public string CategoryId { get; init; } = null!;
}