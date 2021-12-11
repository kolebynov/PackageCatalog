﻿using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Interfaces;

public interface IPackageCatalogService
{
	Task<IReadOnlyCollection<Category>> GetCategories(Pagination? pagination, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<Package>> GetPackages(StringId? categoryId, Pagination? pagination,
		CancellationToken cancellationToken);

	Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsDesc(StringId packageId, Pagination? pagination,
		CancellationToken cancellationToken);

	Task<PackageVersion> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken);

	Task<Stream> GetPackageVersionContent(StringId packageId, Version version, CancellationToken cancellationToken);
}