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
public class PackagesController : ControllerBase
{
	private readonly IPackageCatalogService packageCatalogService;
	private readonly ISkipTokenGenerator skipTokenGenerator;
	private readonly ITempContentStorage tempContentStorage;

	public PackagesController(IPackageCatalogService packageCatalogService, ISkipTokenGenerator skipTokenGenerator,
		ITempContentStorage tempContentStorage)
	{
		this.packageCatalogService =
			packageCatalogService ?? throw new ArgumentNullException(nameof(packageCatalogService));
		this.skipTokenGenerator = skipTokenGenerator ?? throw new ArgumentNullException(nameof(skipTokenGenerator));
		this.tempContentStorage = tempContentStorage ?? throw new ArgumentNullException(nameof(tempContentStorage));
	}

	[HttpGet]
	[MapToApiVersion("1.0")]
	public async Task<CollectionResponseV1<PackageV1>> GetPackages(
		[FromQuery] PaginationV1 pagination, CancellationToken cancellationToken)
	{
		var packages = await packageCatalogService.GetPackages(
			new GetItemsQuery<Package> { Pagination = pagination.ToPaginationObject(skipTokenGenerator) },
			cancellationToken);

		return packages
			.Select(x => x.ToContractV1())
			.ToArray()
			.ToCollectionResponseV1(this, skipTokenGenerator, pagination);
	}

	[HttpPost]
	[MapToApiVersion("1.0")]
	public async Task<PackageV1> AddPackage(
		[FromBody] AddPackageRequestV1 addPackageRequestV1, CancellationToken cancellationToken)
	{
		var package = await packageCatalogService.AddPackage(
			new AddPackageData(new StringId(addPackageRequestV1.PackageId), addPackageRequestV1.DisplayName,
				new StringId(addPackageRequestV1.CategoryId)),
			cancellationToken);
		return package.ToContractV1();
	}

	[HttpGet("{packageId}/versions")]
	[MapToApiVersion("1.0")]
	public async Task<CollectionResponseV1<PackageVersionV1>> GetPackageVersions(
		string packageId, [FromQuery] PaginationV1 pagination, CancellationToken cancellationToken)
	{
		var packageVersions = await packageCatalogService.GetPackageVersionsDesc(
			new StringId(packageId),
			new GetItemsQuery<PackageVersion> { Pagination = pagination.ToPaginationObject(skipTokenGenerator) },
			cancellationToken);

		return packageVersions
			.Select(x => x.ToContractV1())
			.ToArray()
			.ToCollectionResponseV1(this, skipTokenGenerator, pagination);
	}

	[HttpGet("{packageId}/versions/{version}/content")]
	[MapToApiVersion("1.0")]
	public async Task<FileStreamResult> GetPackageVersionContent(
		string packageId, Version version, CancellationToken cancellationToken)
	{
		var stream = await packageCatalogService.GetPackageVersionContent(new StringId(packageId), version,
			cancellationToken);
		return File(stream, "application/octet-stream");
	}

	[HttpPost("{packageId}/versions")]
	[MapToApiVersion("1.0")]
	public async Task<PackageVersionV1> AddPackageVersion(
		string packageId, [FromBody] AddPackageVersionRequestV1 addPackageVersionRequestV1,
		CancellationToken cancellationToken)
	{
		var content = await tempContentStorage.GetTempContent(
			addPackageVersionRequestV1.UploadedContentTicket, cancellationToken);
		var newPackageVersion = await packageCatalogService.AddPackageVersion(
			new AddPackageVersionData(new StringId(packageId), addPackageVersionRequestV1.Version,
				addPackageVersionRequestV1.AdditionalData, content),
			cancellationToken);

		return newPackageVersion.ToContractV1();
	}
}