using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PackageCatalog.Core.Extensions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.EfRepository.Internal;

internal class Repository<T> : IRepository<T> where T : class
{
	private readonly PackageCatalogDbContext context;
	private readonly ILogger<Repository<T>> logger;
	private readonly DbSet<T> dbSet;

	public Repository(PackageCatalogDbContext context, ILogger<Repository<T>> logger)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		dbSet = context.Set<T>();
	}

	public async Task<IReadOnlyCollection<T>> GetItems(Expression<Func<T, bool>>? predicate, Pagination? pagination,
		CancellationToken cancellationToken)
	{
		var query = dbSet.AsQueryable();
		if (predicate != null)
		{
			query = query.Where(predicate);
		}

		return await query.ApplyPagination(pagination).ToArrayAsync(cancellationToken);
	}

	public async Task Add(T item, CancellationToken cancellationToken)
	{
		await dbSet.AddAsync(item, cancellationToken);
		await context.SaveChangesAsync(cancellationToken);
	}

	public virtual async Task Update(T entity, CancellationToken cancellationToken)
	{
		dbSet.Update(entity);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task Delete(T entity, CancellationToken cancellationToken)
	{
		dbSet.Remove(entity);
		await context.SaveChangesAsync(cancellationToken);
	}
}