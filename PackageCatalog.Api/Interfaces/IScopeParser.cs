using PackageCatalog.Api.Objects;

namespace PackageCatalog.Api.Interfaces;

public interface IScopeParser
{
	Scope ParseScope(string scopeStr);
}