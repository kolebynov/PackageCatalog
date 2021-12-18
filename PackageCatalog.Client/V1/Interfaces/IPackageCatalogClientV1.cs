using PackageCatalog.Client.V1.Objects;
using PackageCatalog.Contracts.V1;

namespace PackageCatalog.Client.V1.Interfaces;

public interface IPackageCatalogClientV1
{
	IAsyncEnumerable<CategoryV1> GetCategories(CancellationToken cancellationToken);

	Task<CategoryV1> AddCategory(AddCategoryRequestV1 addCategoryRequestV1, CancellationToken cancellationToken);

	IAsyncEnumerable<PackageV1> GetCategoryPackages(string categoryId, CancellationToken cancellationToken);

	IAsyncEnumerable<PackageV1> GetPackages(CancellationToken cancellationToken);

	Task<PackageV1> AddPackage(AddPackageRequestV1 addPackageRequestV1, CancellationToken cancellationToken);

	IAsyncEnumerable<PackageVersionV1> GetPackageVersions(string packageId, CancellationToken cancellationToken);

	Task<PackageVersionV1> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken);

	Task<Stream> GetPackageVersionContent(string packageId, Version version, CancellationToken cancellationToken);
}