using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Extensions;

public static class EnumerableExtensions
{
	public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, Pagination? pagination)
	{
		if (pagination?.Skip > 0)
		{
			queryable = queryable.Skip(pagination.Skip);
		}

		if (pagination?.Top > 0)
		{
			queryable = queryable.Take(pagination.Top);
		}

		return queryable;
	}

	public static IEnumerable<T> ApplyPagination<T>(this IEnumerable<T> queryable, Pagination? pagination)
	{
		if (pagination?.Skip > 0)
		{
			queryable = queryable.Skip(pagination.Skip);
		}

		if (pagination?.Top > 0)
		{
			queryable = queryable.Take(pagination.Top);
		}

		return queryable;
	}
}