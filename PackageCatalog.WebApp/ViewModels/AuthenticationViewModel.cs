using System.ComponentModel.DataAnnotations;

namespace PackageCatalog.WebApp.ViewModels;

public class AuthenticationViewModel
{
	[Required]
	public string? AccessToken { get; set; }
}