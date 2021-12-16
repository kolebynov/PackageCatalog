using PackageCatalog.Core.Exceptions;

namespace PackageCatalog.Api.Exceptions;

public class ForbiddenPackageCatalogException : PackageCatalogException
{
    public ForbiddenPackageCatalogException(string message)
        : base(message)
    {
    }

    public ForbiddenPackageCatalogException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ForbiddenPackageCatalogException()
        : base("You don't have permissions")
    {
    }
}