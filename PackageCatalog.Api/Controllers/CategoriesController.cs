﻿using Microsoft.AspNetCore.Mvc;
using PackageCatalog.Api.Extensions;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Contracts.V1;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
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
	[ProducesResponseType(typeof(CollectionResponseV1<CategoryV1>), StatusCodes.Status200OK)]
	[MapToApiVersion("1.0")]
	public async Task<CollectionResponseV1<CategoryV1>> GetCategories(
		[FromQuery] PaginationV1 pagination, CancellationToken cancellationToken)
	{
		return (await packageCatalogService.GetCategories(pagination.ToPaginationObject(skipTokenGenerator), cancellationToken))
			.Select(x => new CategoryV1
			{
				Id = x.Id.Value,
				DisplayName = x.DisplayName,
			})
			.ToArray()
			.ToCollectionResponseV1(this, skipTokenGenerator, pagination);
	}

	[HttpGet("{categoryId}/packages")]
	[ProducesResponseType(typeof(CollectionResponseV1<PackageV1>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	[MapToApiVersion("1.0")]
	public async Task<IActionResult> GetPackages(string categoryId, [FromQuery] PaginationV1 pagination,
		CancellationToken cancellationToken)
	{
		if (await packageCatalogService.FindCategory(new StringId(categoryId), cancellationToken) == null)
		{
			return Problem($"Category \"{categoryId}\" not found", statusCode: StatusCodes.Status404NotFound);
		}

		var response = (await packageCatalogService.GetPackages(
				new StringId(categoryId), pagination.ToPaginationObject(skipTokenGenerator), cancellationToken))
			.Select(x => new PackageV1
			{
				Id = x.Id.Value,
				DisplayName = x.DisplayName,
			})
			.ToArray()
			.ToCollectionResponseV1(this, skipTokenGenerator, pagination);

		return Ok(response);
	}
}