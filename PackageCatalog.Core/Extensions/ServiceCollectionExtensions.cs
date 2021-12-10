using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Internal;
using PackageCatalog.Shared.Extensions;

namespace PackageCatalog.Core.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCorePackageServices(this IServiceCollection services)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));
		services.AddSharedPackageServices();

		services.TryAddScoped<IRepositoryFacade, RepositoryFacade>();
		services.TryAddScoped<IPackageCatalogService, PackageCatalogService>();

		return services;
	}
}