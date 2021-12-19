using System.ComponentModel.DataAnnotations;
using PackageCatalog.Contracts;

namespace PackageCatalog.WebApp.ViewModels;

public class AddCategoryViewModel
{
	[Required]
	[RegularExpression(Constants.IdRegex)]
	public string Id { get; set; } = null!;

	public string? DisplayName { get; set; }
}