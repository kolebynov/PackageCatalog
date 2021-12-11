using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.FileSystemStorage.Configuration;
using PackageCatalog.FileSystemStorage.Internal;
using PackageCatalog.Shared.Interfaces;

namespace PackageCatalog.FileSystemStorage.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddFileSystemPackageStorage(
		this IServiceCollection services, Action<FileSystemStorageSettings> optionsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Configure(optionsAction);

		services.AddSingleton<IPackageStorage>(sp =>
		{
			var storagePath = Path.GetFullPath(
				sp.GetRequiredService<IOptions<FileSystemStorageSettings>>().Value.Path);
			sp.GetRequiredService<IFileSystemAdapter>().EnsureDirectoryExists(storagePath);
			return ActivatorUtilities.CreateInstance<PackageStorage>(sp, new PhysicalFileProvider(storagePath));
		});
		return services;
	}
}