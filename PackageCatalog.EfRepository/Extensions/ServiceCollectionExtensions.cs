using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

		services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

		return services;
	}
}