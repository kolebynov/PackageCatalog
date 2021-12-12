using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PackageCatalog.Core.Extensions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.EfRepository.Internal;

internal class Repository<T> : IRepository<T> where T : class
{
	private readonly PackageCatalogDbContext context;
	private readonly DbSet<T> dbSet;

	public Repository(PackageCatalogDbContext context)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
		dbSet = context.Set<T>();
	}

	public async Task<IReadOnlyCollection<T>> GetItems(GetRepositoryItemsQuery<T>? getItemsQuery, CancellationToken cancellationToken)
	{
		return await ApplyQuery(dbSet, getItemsQuery).ToArrayAsync(cancellationToken);
	}

	public async Task Add(T item, CancellationToken cancellationToken)
	{
		await dbSet.AddAsync(item, cancellationToken);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task Update(T entity, CancellationToken cancellationToken)
	{
		dbSet.Update(entity);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task Delete(T entity, CancellationToken cancellationToken)
	{
		dbSet.Remove(entity);
		await context.SaveChangesAsync(cancellationToken);
	}

	private static IQueryable<T> ApplyQuery(IQueryable<T> queryable, GetRepositoryItemsQuery<T>? getItemsQuery)
	{
		if (getItemsQuery == null)
		{
			return queryable;
		}

		if (getItemsQuery.Filters?.Any() == true)
		{
			queryable = getItemsQuery.Filters.Aggregate(queryable, (current, filter) => current.Where(filter));
		}

		if (getItemsQuery.Orderings?.Any() == true)
		{
			var orderedQueryable = Order(queryable, getItemsQuery.Orderings.First());
			queryable = getItemsQuery.Orderings.Skip(1).Aggregate(orderedQueryable, Order);
		}

		return queryable.ApplyPagination(getItemsQuery.Pagination);
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