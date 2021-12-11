using Microsoft.EntityFrameworkCore;
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

	public async Task<IReadOnlyCollection<T>> GetItems(GetItemsQuery<T>? getItemsQuery, CancellationToken cancellationToken)
	{
		var query = getItemsQuery?.ApplyQuery(dbSet.AsQueryable()) ?? dbSet.AsQueryable();
		return await query.ToArrayAsync(cancellationToken);
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
}