using System.Linq.Expressions;
using PackageCatalog.Core.Extensions;

namespace PackageCatalog.Core.Objects;

public class GetItemsQuery<T>
{
	public Pagination? Pagination { get; init; }

	public Expression<Func<T, bool>>? Filter { get; init; }

	public IQueryable<T> ApplyQuery(IQueryable<T> queryable)
	{
		if (Filter != null)
		{
			queryable = queryable.Where(Filter);
		}

		return queryable.ApplyPagination(Pagination);
	}
}