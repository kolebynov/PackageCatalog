using PackageCatalog.Api.Exceptions;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Objects;
using PackageCatalog.Core.Exceptions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Api.Internal;

internal class ScopePackageCatalogService : IPackageCatalogService
{
	private readonly IPackageCatalogService innerService;
	private readonly IScopeAccessor scopeAccessor;

	public ScopePackageCatalogService(IPackageCatalogService innerService, IScopeAccessor scopeAccessor)
	{
		this.innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
		this.scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
	}

	public Task<IReadOnlyCollection<Category>> GetCategories(GetItemsQuery<Category>? query, CancellationToken cancellationToken)
	{
		var getScope = scopeAccessor.Scope.FindInnerScope(ScopeConstants.Get)
			?.FindInnerScope(ScopeConstants.Categories);
		if (getScope == null)
		{
			return Task.FromResult<IReadOnlyCollection<Category>>(Array.Empty<Category>());
		}

		if (getScope.HasFullScope)
		{
			return innerService.GetCategories(query, cancellationToken);
		}

		var getQuery = query ?? new GetItemsQuery<Category>();
		var allowedCategories = getScope.Select(x => new StringId(x.Name)).ToArray();
		getQuery.Filters.Add(x => allowedCategories.Contains(x.Id));
		return innerService.GetCategories(getQuery, cancellationToken);
	}

	public Task<Category> AddCategory(AddCategoryData addCategoryData, CancellationToken cancellationToken)
	{
		var addScope = scopeAccessor.Scope.FindInnerScope(ScopeConstants.Add)
			?.FindInnerScope(ScopeConstants.Categories);
		if (addScope == null)
		{
			throw new ForbiddenPackageCatalogException();
		}

		return innerService.AddCategory(addCategoryData, cancellationToken);
	}

	public Task<IReadOnlyCollection<Package>> GetPackages(GetItemsQuery<Package>? query, CancellationToken cancellationToken)
	{
		var getCategoriesScope = scopeAccessor.Scope.FindInnerScope(ScopeConstants.Get)
			?.FindInnerScope(ScopeConstants.Categories);
		var getPackagesScope = scopeAccessor.Scope.FindInnerScope(ScopeConstants.Get)
			?.FindInnerScope(ScopeConstants.Packages);
		if (getCategoriesScope == null || getPackagesScope == null)
		{
			return Task.FromResult<IReadOnlyCollection<Package>>(Array.Empty<Package>());
		}

		if (getCategoriesScope.HasFullScope && getPackagesScope.HasFullScope)
		{
			return innerService.GetPackages(query, cancellationToken);
		}

		var getQuery = query ?? new GetItemsQuery<Package>();
		if (!getCategoriesScope.HasFullScope)
		{
			var allowedCategories = getCategoriesScope.Select(y => new StringId(y.Name)).ToArray();
			getQuery.Filters.Add(x => allowedCategories.Contains(x.CategoryId));
		}

		if (!getPackagesScope.HasFullScope)
		{
			var allowedPackages = getPackagesScope.Select(x => new StringId(x.Name)).ToArray();
			getQuery.Filters.Add(x => allowedPackages.Contains(x.Id));
		}

		return innerService.GetPackages(getQuery, cancellationToken);
	}

	public Task<Package> AddPackage(AddPackageData addPackageData, CancellationToken cancellationToken)
	{
		var scope = scopeAccessor.Scope;
		if (scope.FindInnerScope(ScopeConstants.Add)?.FindInnerScope(ScopeConstants.Packages) == null)
		{
			throw new ForbiddenPackageCatalogException();
		}

		if (scope.FindInnerScope(ScopeConstants.Get)?.FindInnerScope(ScopeConstants.Categories)
			    ?.FindInnerScope(addPackageData.CategoryId.Value) == null)
		{
			throw NotFoundPackageCatalogException.CreateCategoryNotFound(addPackageData.CategoryId);
		}

		return innerService.AddPackage(addPackageData, cancellationToken);
	}

	public async Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsDesc(
		StringId packageId, GetItemsQuery<PackageVersion>? query, CancellationToken cancellationToken)
	{
		await CheckPackageGetAccess(packageId, scopeAccessor.Scope, cancellationToken);
		if (scopeAccessor.Scope.FindInnerScope(ScopeConstants.Get)
			    ?.FindInnerScope(ScopeConstants.PackageVersions)?.HasFullScope != true)
		{
			return Array.Empty<PackageVersion>();
		}

		return await innerService.GetPackageVersionsDesc(packageId, query, cancellationToken);
	}

	public async Task<PackageVersion> AddPackageVersion(
		AddPackageVersionData addPackageVersionData, CancellationToken cancellationToken)
	{
		if (scopeAccessor.Scope.FindInnerScope(ScopeConstants.Add)?.FindInnerScope(ScopeConstants.PackageVersions) == null)
		{
			throw new ForbiddenPackageCatalogException();
		}

		await CheckPackageGetAccess(addPackageVersionData.PackageId, scopeAccessor.Scope, cancellationToken);
		return await innerService.AddPackageVersion(addPackageVersionData, cancellationToken);
	}

	public async Task<Stream> GetPackageVersionContent(StringId packageId, Version version,
		CancellationToken cancellationToken)
	{
		await CheckPackageVersionGetAccess(packageId, version, scopeAccessor.Scope, cancellationToken);
		return await innerService.GetPackageVersionContent(packageId, version, cancellationToken);
	}

	private async Task CheckPackageGetAccess(StringId packageId, Scope scope, CancellationToken cancellationToken)
	{
		var package = (await innerService.GetPackages(
			new GetItemsQuery<Package> { Filters = { x => x.Id.Equals(packageId) } },
			cancellationToken)).FirstOrDefault();
		if (package == null)
		{
			throw NotFoundPackageCatalogException.CreatePackageNotFound(packageId);
		}

		var requiredScope = new Scope(
			ScopeConstants.Get,
			new Scope(ScopeConstants.Categories, new Scope(package.CategoryId.Value)),
			new Scope(ScopeConstants.Packages, new Scope(packageId.Value)));
		if (!scope.ContainsScope(requiredScope))
		{
			throw NotFoundPackageCatalogException.CreatePackageNotFound(packageId);
		}
	}

	private async Task CheckPackageVersionGetAccess(StringId packageId, Version version, Scope scope,
		CancellationToken cancellationToken)
	{
		await CheckPackageGetAccess(packageId, scope, cancellationToken);
		var requiredScope = new Scope(
			ScopeConstants.Get, new Scope(ScopeConstants.PackageVersions, new Scope($"{packageId}_{version}")));
		if (!scope.ContainsScope(requiredScope))
		{
			throw NotFoundPackageCatalogException.CreatePackageVersionNotFound(packageId, version);
		}
	}
}