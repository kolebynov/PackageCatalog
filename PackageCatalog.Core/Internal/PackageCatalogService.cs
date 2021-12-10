using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PackageCatalog.Core.Extensions;
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
		repositoryFacade.Categories.GetItems(null, pagination, cancellationToken);

	public async Task<Category?> FindCategory(string categoryId, CancellationToken cancellationToken) =>
		(await repositoryFacade.Categories.GetItems(x => x.Id == categoryId, null, cancellationToken))
			.FirstOrDefault();

	public Task<IReadOnlyCollection<Package>> GetPackages(string? categoryId, Pagination? pagination,
		CancellationToken cancellationToken)
	{
		Expression<Func<Package, bool>>? predicate = !string.IsNullOrEmpty(categoryId)
			? x => x.CategoryId == categoryId
			: null;
		return repositoryFacade.Packages.GetItems(predicate, pagination, cancellationToken);
	}

	public async Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsDesc(string packageId, Pagination? pagination,
		CancellationToken cancellationToken)
	{
		var packages = await repositoryFacade.PackageVersions.GetItems(x => x.PackageId == packageId, null,
			cancellationToken);
		return packages.OrderByDescending(x => x.Version).ApplyPagination(pagination).ToArray();
	}

	public async Task<Stream> GetPackageVersionData(PackageVersion packageVersion, CancellationToken cancellationToken) =>
		await packageStorage.GetPackageData(
			await GetPackageStoragePath(packageVersion, cancellationToken),
			cancellationToken);

	private async Task<string> GetPackageStoragePath(PackageVersion packageVersion, CancellationToken cancellationToken)
	{
		var package = (await repositoryFacade.Packages.GetItems(x => x.Id == packageVersion.PackageId, null,
			cancellationToken)).First();
		return $"/{package.CategoryId}/{packageVersion.PackageId}_{packageVersion.Version}";
	}
}