using System.Text.Json;
using PackageCatalog.Api.Interfaces;

namespace PackageCatalog.Api.Internal;

internal class SkipTokenGenerator : ISkipTokenGenerator
{
	public string GenerateSkipToken<T>(T skipTokenObject)
	{
		if (skipTokenObject == null)
		{
			throw new ArgumentNullException(nameof(skipTokenObject));
		}

		return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(skipTokenObject));
	}

	public T ParseSkipToken<T>(string skipToken)
	{
		if (string.IsNullOrEmpty(skipToken))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(skipToken));
		}

		return JsonSerializer.Deserialize<T>(Convert.FromBase64String(skipToken))!;
	}
}