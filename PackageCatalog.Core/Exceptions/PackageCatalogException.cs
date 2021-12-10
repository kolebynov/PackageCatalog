namespace PackageCatalog.Core.Exceptions;

public class PackageCatalogException : Exception
{
    public PackageCatalogException(string message)
        : base(message)
    {
    }

    public PackageCatalogException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PackageCatalogException()
    {
    }
}