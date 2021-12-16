using System.Linq.Expressions;
using PackageCatalog.Core.Models;
using PackageCatalog.EfRepository.Internal.Interfaces;

namespace PackageCatalog.EfRepository.Internal;

internal class PackageVersionInfoProvider : IModelInfoProvider<PackageVersion>
{
	public Expression<Func<PackageVersion, object>> DefaultOrderKey => x => new { x.PackageId, x.Version };
}