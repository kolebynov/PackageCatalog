using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Objects;

namespace PackageCatalog.Api.Internal;

public class ScopeParser : IScopeParser
{
	private const string RootScopeName = "root";

	public Scope ParseScope(string scopeStr)
	{
		if (string.IsNullOrEmpty(scopeStr))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(scopeStr));
		}

		if (scopeStr == "*")
		{
			return new Scope(RootScopeName, Array.Empty<Scope>());
		}

		var scopes = scopeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var resultScope = new ScopeBuilder(RootScopeName);

		foreach (var scope in scopes)
		{
			var scopeParts = scope.Split('/', StringSplitOptions.RemoveEmptyEntries);
			var currentScope = resultScope;
			foreach (var scopePart in scopeParts)
			{
				var innerScope = currentScope.FindInnerScope(scopePart);
				if (innerScope == null)
				{
					currentScope.Add(innerScope = new ScopeBuilder(scopePart));
				}

				currentScope = innerScope;
			}
		}

		return resultScope.Build();
	}

	private class ScopeBuilder
	{
		private readonly string name;
		private readonly List<ScopeBuilder> innerScopes = new();

		public ScopeBuilder(string name)
		{
			this.name = name;
		}

		public void Add(ScopeBuilder scopeBuilder) => innerScopes.Add(scopeBuilder);

		public ScopeBuilder? FindInnerScope(string innerScope) =>
			innerScopes.Find(x => x.name.Equals(innerScope, StringComparison.Ordinal));

		public Scope Build() => new(name, innerScopes.Select(x => x.Build()).ToArray());
	}
}