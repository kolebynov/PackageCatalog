using System.Collections;

namespace PackageCatalog.Api.Objects;

public sealed class Scope : IReadOnlyCollection<Scope>
{
	private readonly IReadOnlyCollection<Scope> innerScopes;

	public string Name { get; }

	public bool HasFullScope => innerScopes.Count == 0;

	public int Count => innerScopes.Count;

	public Scope(string name, IReadOnlyCollection<Scope> innerScopes)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(name));
		}

		Name = name;
		this.innerScopes = innerScopes ?? throw new ArgumentNullException(nameof(innerScopes));
	}

	public Scope(string name, params Scope[] innerScopes)
		: this(name, (IReadOnlyCollection<Scope>)innerScopes)
	{
	}

	public Scope? FindInnerScope(string innerScope) =>
		HasFullScope
			? new Scope(innerScope, Array.Empty<Scope>())
			: innerScopes.FirstOrDefault(x => x.Name.Equals(innerScope, StringComparison.Ordinal));

	public bool MatchesScope(Scope otherScope)
	{
		if (!Name.Equals(otherScope.Name, StringComparison.Ordinal))
		{
			return false;
		}

		return HasFullScope || otherScope.Aggregate(true, (res, other) => res && ContainsScope(other));
	}

	public bool ContainsScope(Scope otherScope)
	{
		if (HasFullScope)
		{
			return true;
		}

		var innerScope = FindInnerScope(otherScope.Name);
		return innerScope != null
			&& otherScope.innerScopes.Aggregate(true, (res, other) => res && innerScope.ContainsScope(other));
	}

	public override string ToString() => Name;

	IEnumerator<Scope> IEnumerable<Scope>.GetEnumerator() => innerScopes.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => innerScopes.GetEnumerator();
}