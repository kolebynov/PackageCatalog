using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.EfRepository.Internal;

namespace PackageCatalog.EfRepository.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddEfPackageRepository(
		this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));
		services.AddDbContext<PackageCatalogDbContext>(optionsAction);

		services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));

		return services;
	}
}