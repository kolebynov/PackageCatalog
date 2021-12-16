using System.Linq.Expressions;

namespace PackageCatalog.EfRepository.Internal.Interfaces;

internal interface IModelInfoProvider<T> where T : class
{
	Expression<Func<T, object>> DefaultOrderKey { get; }
}