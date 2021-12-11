using System.Linq.Expressions;
using PackageCatalog.Core.Extensions;

namespace PackageCatalog.Core.Objects;

public class GetItemsQuery<T>
{
	public Pagination? Pagination { get; init; }

	public Expression<Func<T, bool>>? Filter { get; init; }

	public IReadOnlyCollection<(OrderDirection Direction, Expression<Func<T, object>> KeySelector)>? Orderings { get; init; }

	public IQueryable<T> ApplyQuery(IQueryable<T> queryable)
	{
		if (Filter != null)
		{
			queryable = queryable.Where(Filter);
		}

		if (Orderings?.Any() == true)
		{
			var orderedQueryable = Order(queryable, Orderings.First());
			queryable = Orderings.Skip(1).Aggregate(orderedQueryable, Order);
		}

		return queryable.ApplyPagination(Pagination);
	}

	private static IOrderedQueryable<T> Order(
		IQueryable<T> queryable, (OrderDirection Direction, Expression<Func<T, object>> KeySelector) ordering)
	{
		return ordering.Direction switch
		{
			OrderDirection.Ascending => queryable.OrderBy(ordering.KeySelector),
			OrderDirection.Descending => queryable.OrderByDescending(ordering.KeySelector),
			_ => throw new ArgumentOutOfRangeException(nameof(ordering.Direction), "Invalid order direction"),
		};
	}

	private static IOrderedQueryable<T> Order(
		IOrderedQueryable<T> queryable, (OrderDirection Direction, Expression<Func<T, object>> KeySelector) ordering)
	{
		return ordering.Direction switch
		{
			OrderDirection.Ascending => queryable.ThenBy(ordering.KeySelector),
			OrderDirection.Descending => queryable.ThenByDescending(ordering.KeySelector),
			_ => throw new ArgumentOutOfRangeException(nameof(ordering.Direction), "Invalid order direction"),
		};
	}
}