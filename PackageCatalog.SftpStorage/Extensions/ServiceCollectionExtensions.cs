using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.SftpStorage.Configuration;
using PackageCatalog.SftpStorage.Internal;
using PackageCatalog.SftpStorage.Internal.Interfaces;

namespace PackageCatalog.SftpStorage.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSftpStorage(
		this IServiceCollection services, Action<SftpStorageSettings> settingsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Configure(settingsAction);

		services.AddSingleton<ISftpClientEx>(sp =>
		{
			var settings = sp.GetRequiredService<IOptions<SftpStorageSettings>>().Value;
			return new SftpClientEx(settings.Host, settings.Port, settings.UserName, settings.Password,
				sp.GetRequiredService<ILogger<SftpClientEx>>());
		});
		services.AddSingleton<IPackageStorage, PackageStorage>();

		return services;
	}
}