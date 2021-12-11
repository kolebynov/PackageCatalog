namespace PackageCatalog.Core.Exceptions;

public class NotFoundPackageCatalogException : PackageCatalogException
{
    public NotFoundPackageCatalogException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundPackageCatalogException(string message)
        : base(message)
    {
    }

    public NotFoundPackageCatalogException()
    {
    }
}