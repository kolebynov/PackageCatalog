using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Internal;

internal class PackageCatalogService : IPackageCatalogService
{
	private readonly IRepositoryFacade repositoryFacade;
	private readonly IPackageStorage packageStorage;
	private readonly ILogger<PackageCatalogService> logger;

	public PackageCatalogService(IRepositoryFacade repositoryFacade, IPackageStorage packageStorage,
		ILogger<PackageCatalogService> logger)
	{
		this.repositoryFacade = repositoryFacade ?? throw new ArgumentNullException(nameof(repositoryFacade));
		this.packageStorage = packageStorage ?? throw new ArgumentNullException(nameof(packageStorage));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public Task<IReadOnlyCollection<Category>> GetCategories(
		Pagination? pagination, CancellationToken cancellationToken) =>
		repositoryFacade.Categories.GetItems(new GetItemsQuery<Category> { Pagination = pagination }, cancellationToken);

	public async Task<IReadOnlyCollection<Package>> GetPackages(StringId? categoryId, Pagination? pagination,
		CancellationToken cancellationToken)
	{
		if (categoryId != null)
		{
			await GetCategory(categoryId, cancellationToken);
		}

		Expression<Func<Package, bool>>? filter = categoryId != null
			? x => x.CategoryId.Equals(categoryId)
			: null;
		return await repositoryFacade.Packages.GetItems(
			new GetItemsQuery<Package> { Filter = filter, Pagination = pagination }, cancellationToken);
	}

	public async Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsDesc(
		StringId packageId, Pagination? pagination, CancellationToken cancellationToken)
	{
		await GetPackage(packageId, cancellationToken);

		var packages = await repositoryFacade.PackageVersions.GetItems(
			new GetItemsQuery<PackageVersion>
			{
				Filter = x => x.PackageId.Equals(packageId),
				Pagination = pagination,
				Orderings = new (OrderDirection, Expression<Func<PackageVersion, object>>)[]
				{
					(OrderDirection.Descending, x => x.Version),
				},
			},
			cancellationToken);
		return packages;
	}

	public async Task<PackageVersion> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken)
	{
		await packageStorage.StorePackageContent(
			await GetPackageStoragePath(addPackageVersionData.PackageId, addPackageVersionData.Version,
				cancellationToken),
			addPackageVersionData.Content, cancellationToken);

		var newPackageVersion = new PackageVersion(addPackageVersionData.PackageId, addPackageVersionData.Version,
			addPackageVersionData.AdditionalData, addPackageVersionData.Content.Length);
		await repositoryFacade.PackageVersions.Add(newPackageVersion, cancellationToken);
		return newPackageVersion;
	}

	public async Task<Stream> GetPackageVersionContent(StringId packageId, Version version,
		CancellationToken cancellationToken)
	{
		var packageVersion = (await repositoryFacade.PackageVersions.GetItems(
				new GetItemsQuery<PackageVersion> { Filter = x => x.PackageId.Equals(packageId) && x.Version.Equals(version) },
				cancellationToken))
			.FirstOrDefault();
		if (packageVersion == null)
		{
			throw new NotFoundPackageCatalogException($"Package {packageId} {version} not found");
		}

		return await packageStorage.GetPackageContent(
			await GetPackageStoragePath(packageVersion.PackageId, packageVersion.Version, cancellationToken),
			cancellationToken);
	}

	private async Task<Category> GetCategory(StringId categoryId, CancellationToken cancellationToken)
	{
		var category = (await repositoryFacade.Categories.GetItems(
				new GetItemsQuery<Category> { Filter = x => x.Id.Equals(categoryId) }, cancellationToken))
			.FirstOrDefault();
		return category ?? throw new NotFoundPackageCatalogException($"Category \"{categoryId}\" not found");
	}

	private async Task<Package> GetPackage(StringId packageId, CancellationToken cancellationToken)
	{
		var package = (await repositoryFacade.Packages.GetItems(
				new GetItemsQuery<Package> { Filter = x => x.Id.Equals(packageId) }, cancellationToken))
			.FirstOrDefault();
		return package ?? throw new NotFoundPackageCatalogException($"Package \"{packageId}\" not found");
	}

	private async Task<string> GetPackageStoragePath(StringId packageId, Version version,
		CancellationToken cancellationToken)
	{
		var package = await GetPackage(packageId, cancellationToken);
		return $"/{package.CategoryId}/{packageId}_{version}";
	}
}