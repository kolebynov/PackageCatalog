namespace PackageCatalog.Api.Interfaces;

public interface ISkipTokenGenerator
{
	string GenerateSkipToken<T>(T skipTokenObject);

	T ParseSkipToken<T>(string skipToken);
}