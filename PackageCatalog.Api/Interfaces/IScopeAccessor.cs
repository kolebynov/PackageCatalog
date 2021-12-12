using PackageCatalog.Api.Objects;

namespace PackageCatalog.Api.Interfaces;

public interface IScopeAccessor
{
	Scope Scope { get; }
}