using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core;

public class PackageCatalogService : IPackageCatalogService
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
		GetItemsQuery<Category>? query, CancellationToken cancellationToken) =>
		repositoryFacade.Categories.GetItems(query?.ToRepositoryQuery(), cancellationToken);

	public async Task<Category> AddCategory(AddCategoryData addCategoryData, CancellationToken cancellationToken)
	{
		var (id, displayName) = addCategoryData;
		var newCategory = new Category(id, displayName);
		await repositoryFacade.Categories.Add(newCategory, cancellationToken);

		return newCategory;
	}

	public Task<IReadOnlyCollection<Package>> GetPackages(
		GetItemsQuery<Package>? query, CancellationToken cancellationToken) =>
		repositoryFacade.Packages.GetItems(query?.ToRepositoryQuery(), cancellationToken);

	public async Task<Package> AddPackage(AddPackageData addPackageData, CancellationToken cancellationToken)
	{
		var (packageId, displayName, categoryId) = addPackageData;
		var categories = await GetCategories(
			new GetItemsQuery<Category> { Filters = { x => x.Id.Equals(categoryId) } },
			cancellationToken);
		if (!categories.Any())
		{
			throw NotFoundPackageCatalogException.CreateCategoryNotFound(categoryId);
		}

		var package = new Package(packageId, displayName, categoryId);
		await repositoryFacade.Packages.Add(package, cancellationToken);
		return package;
	}

	public async Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsDesc(
		StringId packageId, GetItemsQuery<PackageVersion>? query, CancellationToken cancellationToken)
	{
		await GetPackage(packageId, cancellationToken);

		var filters = query?.Filters ?? new List<Expression<Func<PackageVersion, bool>>>();
		filters.Add(x => x.PackageId.Equals(packageId));

		return await repositoryFacade.PackageVersions.GetItems(
			new GetRepositoryItemsQuery<PackageVersion>
			{
				Filters = new ReadOnlyCollection<Expression<Func<PackageVersion, bool>>>(filters),
				Pagination = query?.Pagination,
				Orderings = new (OrderDirection, Expression<Func<PackageVersion, object>>)[]
				{
					(OrderDirection.Descending, x => x.Version),
				},
			},
			cancellationToken);
	}

	public async Task<PackageVersion> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken)
	{
		var (packageId, version, additionalData, content) = addPackageVersionData;
		await packageStorage.StorePackageContent(
			await GetPackageStoragePath(packageId, version, cancellationToken),
			content, cancellationToken);

		var newPackageVersion = new PackageVersion(packageId, version, additionalData, content.Length);
		await repositoryFacade.PackageVersions.Add(newPackageVersion, cancellationToken);
		return newPackageVersion;
	}

	public async Task<Stream> GetPackageVersionContent(StringId packageId, Version version,
		CancellationToken cancellationToken)
	{
		var packageVersion = (await repositoryFacade.PackageVersions.GetItems(
				new GetRepositoryItemsQuery<PackageVersion> { Filters = GetPackageVersionFilter(packageId, version) },
				cancellationToken))
			.FirstOrDefault();

		if (packageVersion == null)
		{
			throw NotFoundPackageCatalogException.CreatePackageVersionNotFound(packageId, version);
		}

		return await packageStorage.GetPackageContent(
			await GetPackageStoragePath(packageVersion.PackageId, packageVersion.Version, cancellationToken),
			cancellationToken);
	}

	private async Task<Package> GetPackage(StringId packageId, CancellationToken cancellationToken)
	{
		var package = (await repositoryFacade.Packages.GetItems(
				new GetRepositoryItemsQuery<Package> { Filters = GetPackageFilter(packageId) }, cancellationToken))
			.FirstOrDefault();
		return package ?? throw NotFoundPackageCatalogException.CreatePackageNotFound(packageId);
	}

	private async Task<string> GetPackageStoragePath(StringId packageId, Version version,
		CancellationToken cancellationToken)
	{
		var package = await GetPackage(packageId, cancellationToken);
		return $"/{package.CategoryId}/{packageId}_{version}";
	}

	private static Expression<Func<Package, bool>>[] GetPackageFilter(StringId packageId) =>
		new Expression<Func<Package, bool>>[] { x => x.Id.Equals(packageId) };

	private static Expression<Func<PackageVersion, bool>>[] GetPackageVersionFilter(StringId packageId, Version version) =>
		new Expression<Func<PackageVersion, bool>>[] { x => x.PackageId.Equals(packageId) && x.Version.Equals(version) };
}