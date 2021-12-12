using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Interfaces;

public interface IRepository<T> where T : class
{
	Task<IReadOnlyCollection<T>> GetItems(GetRepositoryItemsQuery<T>? getItemsQuery, CancellationToken cancellationToken);

	Task Add(T item, CancellationToken cancellationToken);

	Task Update(T item, CancellationToken cancellationToken);

	Task Delete(T item, CancellationToken cancellationToken);
}