namespace PackageCatalog.Core.Objects;

public class Pagination
{
	public int Top { get; }

	public int Skip { get; }

	public Pagination(int top, int skip)
	{
		if (top < 0)
		{
			throw new ArgumentException("Top can't be less than zero", nameof(top));
		}

		if (skip < 0)
		{
			throw new ArgumentException("Skip can't be less than zero", nameof(skip));
		}

		Top = top;
		Skip = skip;
	}
}