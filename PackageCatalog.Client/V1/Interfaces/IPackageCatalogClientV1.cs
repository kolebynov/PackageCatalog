using PackageCatalog.Client.V1.Objects;
using PackageCatalog.Contracts.V1;

namespace PackageCatalog.Client.V1.Interfaces;

public interface IPackageCatalogClientV1
{
	IAsyncEnumerable<CategoryV1> GetCategories(CancellationToken cancellationToken);

	Task<CategoryV1> AddCategory(AddCategoryRequestV1 addCategoryRequestV1, CancellationToken cancellationToken);

	Task<PackageVersionV1> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken);
}