using Microsoft.AspNetCore.Mvc;
using PackageCatalog.Api.Dto;
using PackageCatalog.Core.Interfaces;

namespace PackageCatalog.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CategoriesController : ControllerBase
{
	private readonly IPackageCatalogService packageCatalogService;

	public CategoriesController(IPackageCatalogService packageCatalogService)
	{
		this.packageCatalogService = packageCatalogService ?? throw new ArgumentNullException(nameof(packageCatalogService));
	}

	[HttpGet]
	public async Task<IReadOnlyCollection<CategoryDto>> GetCategories(CancellationToken cancellationToken)
	{
		return (await packageCatalogService.GetCategories(null, cancellationToken))
			.Select(x => new CategoryDto
			{
				Id = x.Id,
				DisplayName = x.DisplayName,
			})
			.ToArray();
	}

	[HttpGet("{categoryId}/packages")]
	public async Task<IActionResult> GetPackages(string categoryId, CancellationToken cancellationToken)
	{
		if (await packageCatalogService.FindCategory(categoryId, cancellationToken) != null)
		{
			return Ok((await packageCatalogService.GetPackages(categoryId, null, cancellationToken))
				.Select(x => new PackageDto
				{
					Id = x.Id,
					DisplayName = x.DisplayName,
				})
				.ToArray());
		}

		return ValidationProblem($"Category \"{categoryId}\" not found", statusCode: StatusCodes.Status404NotFound);
	}
}