using System.Linq.Expressions;
using PackageCatalog.Core.Models;
using PackageCatalog.EfRepository.Internal.Interfaces;

namespace PackageCatalog.EfRepository.Internal;

internal class CategoryInfoProvider : IModelInfoProvider<Category>
{
	public Expression<Func<Category, object>> DefaultOrderKey => x => x.Id;
}