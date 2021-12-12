using System.Linq.Expressions;

namespace PackageCatalog.Core.Objects;

public class GetItemsQuery<T>
{
	public Pagination? Pagination { get; init; }

	public IList<Expression<Func<T, bool>>> Filters { get; } = new List<Expression<Func<T, bool>>>();

	internal GetRepositoryItemsQuery<T> ToRepositoryQuery() => new()
	{
		Filters = (IReadOnlyCollection<Expression<Func<T, bool>>>)Filters,
		Pagination = Pagination,
	};
}