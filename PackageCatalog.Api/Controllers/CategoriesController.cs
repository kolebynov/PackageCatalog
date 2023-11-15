using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageCatalog.Api.Extensions;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Contracts.V1;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
	private readonly IPackageCatalogService packageCatalogService;
	private readonly ISkipTokenGenerator skipTokenGenerator;

	public CategoriesController(IPackageCatalogService packageCatalogService, ISkipTokenGenerator skipTokenGenerator)
	{
		this.packageCatalogService = packageCatalogService ?? throw new ArgumentNullException(nameof(packageCatalogService));
		this.skipTokenGenerator = skipTokenGenerator ?? throw new ArgumentNullException(nameof(skipTokenGenerator));
	}

	[HttpGet]
	[MapToApiVersion("1.0")]
	public async Task<CollectionResponseV1<CategoryV1>> GetCategories(
		[FromQuery] PaginationV1 pagination, CancellationToken cancellationToken)
	{
		return (await packageCatalogService.GetCategories(
				new GetItemsQuery<Category> { Pagination = pagination.ToPaginationObject(skipTokenGenerator) },
				cancellationToken))
			.Select(x => x.ToContractV1())
			.ToArray()
			.ToCollectionResponseV1(this, skipTokenGenerator, pagination);
	}

	[HttpPost]
	[MapToApiVersion("1.0")]
	public async Task<CategoryV1> AddCategory(
		[FromBody] AddCategoryRequestV1 addCategoryRequestV1, CancellationToken cancellationToken)
	{
		var category = await packageCatalogService.AddCategory(
			new AddCategoryData(new StringId(addCategoryRequestV1.Id), addCategoryRequestV1.DisplayName),
			cancellationToken);
		return category.ToContractV1();
	}

	[HttpGet("{categoryId}/packages")]
	[ProducesResponseType(typeof(CollectionResponseV1<PackageV1>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	[MapToApiVersion("1.0")]
	public async Task<IActionResult> GetPackages(string categoryId, [FromQuery] PaginationV1 pagination,
		CancellationToken cancellationToken)
	{
		var categories = await packageCatalogService.GetCategories(
			new GetItemsQuery<Category> { Filters = { x => x.Id.Equals(new StringId(categoryId)) } },
			cancellationToken);
		if (categories.Count == 0)
		{
			return Problem($"Category \"{categoryId}\" not found", statusCode: StatusCodes.Status404NotFound);
		}

		var response = (await packageCatalogService.GetPackages(
				new GetItemsQuery<Package>
				{
					Filters = { x => x.CategoryId.Equals(new StringId(categoryId)) },
					Pagination = pagination.ToPaginationObject(skipTokenGenerator),
				},
				cancellationToken))
			.Select(x => x.ToContractV1())
			.ToArray()
			.ToCollectionResponseV1(this, skipTokenGenerator, pagination);

		return Ok(response);
	}
}