namespace PackageCatalog.Core.Interfaces;

public interface IPackageStorage
{
	Task<Stream> GetPackageData(string path, CancellationToken cancellationToken);

	Task StorePackageData(string path, Stream packageData, CancellationToken cancellationToken);
}