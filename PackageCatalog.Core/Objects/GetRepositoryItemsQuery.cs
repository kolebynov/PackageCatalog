using System.Linq.Expressions;

namespace PackageCatalog.Core.Objects;

public class GetRepositoryItemsQuery<T>
{
	public Pagination? Pagination { get; init; }

	public IReadOnlyCollection<Expression<Func<T, bool>>>? Filters { get; init; }

	public IReadOnlyCollection<(OrderDirection Direction, Expression<Func<T, object>> KeySelector)>? Orderings { get; init; }
}