using System.Text.Json.Serialization;

namespace PackageCatalog.Contracts.V1;

public class CollectionResponseV1<T>
{
	public IReadOnlyCollection<T> Data { get; init; } = null!;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public Uri? NextUri { get; init; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? SkipToken { get; init; }
}