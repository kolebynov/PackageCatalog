namespace PackageCatalog.Api;

public sealed class GenerateJwtTokenParams
{
	public string Scope { get; init; } = null!;

	public string Role { get; init; } = null!;
}