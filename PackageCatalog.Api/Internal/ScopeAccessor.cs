using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Objects;

namespace PackageCatalog.Api.Internal;

internal class ScopeAccessor : IScopeAccessor
{
	private readonly IScopeParser scopeParser;
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly ILogger<ScopeAccessor> logger;
	private Scope? scope;

	public Scope Scope => scope ??= ParseScope();

	public ScopeAccessor(IScopeParser scopeParser, IHttpContextAccessor httpContextAccessor, ILogger<ScopeAccessor> logger)
	{
		this.scopeParser = scopeParser ?? throw new ArgumentNullException(nameof(scopeParser));
		this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	private Scope ParseScope()
	{
		var scopeStr = httpContextAccessor.HttpContext!.User.FindFirst("scope")?.Value!;
		logger.LogInformation("Trying to parse scope: {ScopeStr}", scopeStr);
		return scopeParser.ParseScope(scopeStr);
	}
}