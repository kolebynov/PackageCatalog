namespace PackageCatalog.Core.Interfaces;

public interface IPackageStorage
{
	Task<Stream> GetPackageContent(string path, CancellationToken cancellationToken);

	Task StorePackageContent(string path, Stream packageData, CancellationToken cancellationToken);
}