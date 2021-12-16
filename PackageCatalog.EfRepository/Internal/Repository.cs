using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Objects;
using PackageCatalog.EfRepository.Internal.Interfaces;

namespace PackageCatalog.EfRepository.Internal;

internal class Repository<T> : IRepository<T> where T : class
{
	private readonly PackageCatalogDbContext context;
	private readonly DbSet<T> dbSet;
	private readonly IModelInfoProvider<T> modelInfoProvider;

	public Repository(PackageCatalogDbContext context, IModelInfoProvider<T> modelInfoProvider)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
		this.modelInfoProvider = modelInfoProvider ?? throw new ArgumentNullException(nameof(modelInfoProvider));
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

	private IQueryable<T> ApplyQuery(IQueryable<T> queryable, GetRepositoryItemsQuery<T>? getItemsQuery)
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
		else if (getItemsQuery.Pagination?.Skip > 0 || getItemsQuery.Pagination?.Top > 0)
		{
			queryable = queryable.OrderBy(modelInfoProvider.DefaultOrderKey);
		}

		return ApplyPagination(queryable, getItemsQuery.Pagination);
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

	private static IQueryable<T> ApplyPagination(IQueryable<T> queryable, Pagination? pagination)
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