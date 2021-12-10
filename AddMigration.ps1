param(
    [Parameter(Mandatory)]
    [string]$Name
)

dotnet ef migrations add $Name -p PackageCatalog.EfRepository -s PackageCatalog.Api