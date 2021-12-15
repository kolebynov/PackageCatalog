using System.Runtime.CompilerServices;
using PackageCatalog.Client.V1.Interfaces;
using PackageCatalog.Contracts.V1;

namespace PackageCatalog.Client.V1.Internal;

internal class PackageCatalogClientV1 : IPackageCatalogClientV1
{
	private readonly ILowLevelClientV1 lowLevelClientV1;

	public PackageCatalogClientV1(ILowLevelClientV1 lowLevelClientV1)
	{
		this.lowLevelClientV1 = lowLevelClientV1 ?? throw new ArgumentNullException(nameof(lowLevelClientV1));
	}

	public IAsyncEnumerable<CategoryV1> GetCategories(CancellationToken cancellationToken) =>
		ExecuteCollectionRequest(p => lowLevelClientV1.GetCategories(p, cancellationToken), cancellationToken);

	public Task<CategoryV1> AddCategory(AddCategoryRequestV1 addCategoryRequestV1, CancellationToken cancellationToken) =>
		lowLevelClientV1.AddCategory(addCategoryRequestV1, cancellationToken);

	private static async IAsyncEnumerable<T> ExecuteCollectionRequest<T>(
		Func<PaginationV1, Task<CollectionResponseV1<T>>> pageProvider,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		CollectionResponseV1<T>? page = null;

		do
		{
			page = await pageProvider(
				new PaginationV1 { Top = 1, SkipToken = page?.SkipToken });
			foreach (var categoryV1 in page.Data)
			{
				yield return categoryV1;
			}
		}
		while (!string.IsNullOrEmpty(page?.SkipToken));
	}
}