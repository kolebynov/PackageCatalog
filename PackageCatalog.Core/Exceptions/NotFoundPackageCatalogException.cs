using PackageCatalog.Core.Objects;

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

    public static NotFoundPackageCatalogException CreateCategoryNotFound(StringId categoryId) =>
        new($"Category \"{categoryId}\" not found");

    public static NotFoundPackageCatalogException CreatePackageNotFound(StringId packageId) =>
        new($"Package \"{packageId}\" not found");

    public static NotFoundPackageCatalogException CreatePackageVersionNotFound(StringId packageId, Version version) =>
        new($"Package {packageId} {version} not found");
}