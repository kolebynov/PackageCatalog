using System.ComponentModel.DataAnnotations;

namespace PackageCatalog.Contracts.V1;

public class PaginationV1
{
	[Range(1, 1000)]
	public int Top { get; init; } = 100;

	[Range(0, int.MaxValue)]
	public int? Skip { get; init; }

	public string? SkipToken { get; init; }
}