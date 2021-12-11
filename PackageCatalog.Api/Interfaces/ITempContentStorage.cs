namespace PackageCatalog.Api.Interfaces;

public interface ITempContentStorage
{
	Task<string> StoreTempContent(Stream stream, CancellationToken cancellationToken);

	Task<Stream> GetTempContent(string ticket, CancellationToken cancellationToken);
}