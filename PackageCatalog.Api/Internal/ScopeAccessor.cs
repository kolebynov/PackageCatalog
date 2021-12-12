using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Objects;

namespace PackageCatalog.Api.Internal;

internal class ScopeAccessor : IScopeAccessor
{
	private readonly IScopeParser scopeParser;
	private readonly IHttpContextAccessor httpContextAccessor;
	private Scope? scope;

	public Scope Scope =>
		scope ??= scopeParser.ParseScope(httpContextAccessor.HttpContext!.User.FindFirst("scope")?.Value!);

	public ScopeAccessor(IScopeParser scopeParser, IHttpContextAccessor httpContextAccessor)
	{
		this.scopeParser = scopeParser ?? throw new ArgumentNullException(nameof(scopeParser));
		this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
	}
}