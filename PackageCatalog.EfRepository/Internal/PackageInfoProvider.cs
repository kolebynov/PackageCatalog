using System.Linq.Expressions;
using PackageCatalog.Core.Models;
using PackageCatalog.EfRepository.Internal.Interfaces;

namespace PackageCatalog.EfRepository.Internal;

internal class PackageInfoProvider : IModelInfoProvider<Package>
{
	public Expression<Func<Package, object>> DefaultOrderKey => x => x.Id;
}