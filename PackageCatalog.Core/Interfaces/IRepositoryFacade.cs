using PackageCatalog.Core.Models;

namespace PackageCatalog.Core.Interfaces;

public interface IRepositoryFacade
{
	public IRepository<Category> Categories { get; }

	public IRepository<Package> Packages { get; }

	public IRepository<PackageVersion> PackageVersions { get; }
}