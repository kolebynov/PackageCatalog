using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.Core.Models;
using PackageCatalog.EfRepository.Internal;
using PackageCatalog.EfRepository.Internal.Interfaces;

namespace PackageCatalog.EfRepository.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddEfPackageRepository(
		this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));
		services.AddDbContext<PackageCatalogDbContext>(optionsAction);

		services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
		services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<PackageCatalogDbContext>());

		services.TryAddSingleton<IModelInfoProvider<Category>, CategoryInfoProvider>();
		services.TryAddSingleton<IModelInfoProvider<Package>, PackageInfoProvider>();
		services.TryAddSingleton<IModelInfoProvider<PackageVersion>, PackageVersionInfoProvider>();

		return services;
	}
}