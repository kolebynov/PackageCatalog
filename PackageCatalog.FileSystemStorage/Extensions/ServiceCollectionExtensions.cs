using Microsoft.Extensions.DependencyInjection;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.FileSystemStorage.Internal;

namespace PackageCatalog.FileSystemStorage.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddFileSystemPackageStorage(this IServiceCollection services)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.AddSingleton<IPackageStorage, PackageStorage>();
		return services;
	}
}