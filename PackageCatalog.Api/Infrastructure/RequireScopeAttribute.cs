using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PackageCatalog.Api.Interfaces;

namespace PackageCatalog.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireScopeAttribute : Attribute, IAuthorizationFilter
{
	public string Scope { get; }

	public RequireScopeAttribute(string scope)
	{
		if (string.IsNullOrEmpty(scope))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(scope));
		}

		Scope = scope;
	}

	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var scopeStr = context.HttpContext.User.FindFirst("scope")?.Value;
		if (string.IsNullOrEmpty(scopeStr))
		{
			SetFailedResult(context);
			return;
		}

		var scopeParser = context.HttpContext.RequestServices.GetRequiredService<IScopeParser>();
		var parsedScope = scopeParser.ParseScope(scopeStr);
		if (!parsedScope.MatchesScope(scopeParser.ParseScope(Scope)))
		{
			SetFailedResult(context);
		}
	}

	private static void SetFailedResult(AuthorizationFilterContext context)
	{
		var details = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>()
			.CreateProblemDetails(context.HttpContext, StatusCodes.Status404NotFound);
		context.Result = new NotFoundObjectResult(details);
	}
}