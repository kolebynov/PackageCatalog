namespace PackageCatalog.Api;

public sealed class GenerateJwtTokenParams
{
	public string Scope { get; init; } = null!;

#pragma warning disable CA1819
	public string[] Roles { get; init; } = null!;
#pragma warning restore CA1819
}