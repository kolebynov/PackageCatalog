using System.Text.RegularExpressions;

namespace PackageCatalog.Core.Objects;

public class StringId : IEquatable<StringId>
{
	private static readonly Regex IdRegex = new("^[A-Za-z]+$", RegexOptions.Compiled);

	public string Value { get; }

	public StringId(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(value));
		}

		if (!IdRegex.IsMatch(value))
		{
			throw new ArgumentException($"Id doesn't match regex pattern {IdRegex}");
		}

		Value = value;
	}

	public override string ToString() => Value;

	public bool Equals(StringId? other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Value.Equals(other.Value, StringComparison.Ordinal);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((StringId)obj);
	}

	public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

	public static StringId? FromNullableString(string? value) =>
		!string.IsNullOrEmpty(value) ? new StringId(value) : null;
}