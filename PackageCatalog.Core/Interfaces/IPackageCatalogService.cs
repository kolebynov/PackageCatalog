using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Interfaces;

public interface IPackageCatalogService
{
	Task<IReadOnlyCollection<Category>> GetCategories(
		GetItemsQuery<Category>? query, CancellationToken cancellationToken);

	Task<Category> AddCategory(AddCategoryData addCategoryData, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<Package>> GetPackages(
		GetItemsQuery<Package>? query, CancellationToken cancellationToken);

	Task<Package> AddPackage(AddPackageData addPackageData, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsDesc(
		StringId packageId, GetItemsQuery<PackageVersion>? query, CancellationToken cancellationToken);

	Task<PackageVersion> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken);

	Task<Stream> GetPackageVersionContent(StringId packageId, Version version, CancellationToken cancellationToken);
}