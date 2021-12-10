using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PackageCatalog.Shared.Interfaces;
using PackageCatalog.Shared.Internal;

namespace PackageCatalog.Shared.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSharedPackageServices(this IServiceCollection services)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));
		services.TryAddSingleton<IFileSystemAdapter, FileSystemAdapter>();

		return services;
	}
}