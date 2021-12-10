using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Models;

namespace PackageCatalog.Core.Internal;

internal class RepositoryFacade : IRepositoryFacade
{
	public IRepository<Category> Categories { get; }

	public IRepository<Package> Packages { get; }

	public IRepository<PackageVersion> PackageVersions { get; }

	public RepositoryFacade(IRepository<Category> categories, IRepository<Package> packages,
		IRepository<PackageVersion> packageVersions)
	{
		Categories = categories ?? throw new ArgumentNullException(nameof(categories));
		Packages = packages ?? throw new ArgumentNullException(nameof(packages));
		PackageVersions = packageVersions ?? throw new ArgumentNullException(nameof(packageVersions));
	}
}