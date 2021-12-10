using PackageCatalog.Core.Interfaces;

namespace PackageCatalog.FileSystemStorage.Internal;

internal class PackageStorage : IPackageStorage
{
	public Task<Stream> GetPackageData(string path, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task StorePackageData(string path, Stream packageData, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}