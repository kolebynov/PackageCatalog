using PackageCatalog.Contracts.V1;
using Refit;

namespace PackageCatalog.Client.V1.Interfaces;

public interface ILowLevelClientV1
{
	[Get("/api/categories")]
	Task<CollectionResponseV1<CategoryV1>> GetCategories(PaginationV1? paginationV1, CancellationToken cancellationToken);

	[Post("/api/categories")]
	Task<CategoryV1> AddCategory([Body] AddCategoryRequestV1 addCategoryRequestV1, CancellationToken cancellationToken);

	[Get("/api/categories/{categoryId}/packages")]
	Task<CollectionResponseV1<PackageV1>> GetCategoryPackages(string categoryId, PaginationV1? paginationV1,
		CancellationToken cancellationToken);

	[Get("/api/packages")]
	Task<CollectionResponseV1<PackageV1>> GetPackages(PaginationV1? paginationV1, CancellationToken cancellationToken);

	[Post("/api/packages")]
	Task<PackageV1> AddPackage([Body] AddPackageRequestV1 addPackageRequestV1, CancellationToken cancellationToken);

	[Get("/api/packages/{packageId}/versions")]
	Task<CollectionResponseV1<PackageVersionV1>> GetPackageVersions(string packageId, PaginationV1? paginationV1,
		CancellationToken cancellationToken);

	[Post("/api/packages/{packageId}/versions")]
	Task<PackageVersionV1> AddPackageVersion(
		string packageId, [Body] AddPackageVersionRequestV1 addPackageVersionRequestV1,
		CancellationToken cancellationToken);

	[Get("/api/packages/{packageId}/versions/{version}/content")]
	Task<Stream> GetPackageVersionContent(string packageId, Version version, CancellationToken cancellationToken);
}