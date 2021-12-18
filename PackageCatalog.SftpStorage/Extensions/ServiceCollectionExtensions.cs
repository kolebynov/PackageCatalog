using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCatalog.Core.Interfaces;
using PackageCatalog.SftpStorage.Configuration;
using PackageCatalog.SftpStorage.Internal;
using Renci.SshNet;

namespace PackageCatalog.SftpStorage.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSftpStorage(
		this IServiceCollection services, Action<SftpStorageSettings> settingsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Configure(settingsAction);

		services.AddSingleton(sp =>
		{
			var settings = sp.GetRequiredService<IOptions<SftpStorageSettings>>().Value;
			var logger = sp.GetRequiredService<ILogger<SftpClientWrapper>>();
			logger.LogInformation(
				"Creating SFTP client. [Host: {Host}][Port: {Port}][User: {User}]",
				settings.Host, settings.Port, settings.UserName);
			return new SftpClientWrapper(
				new SftpClient(settings.Host, settings.Port, settings.UserName, settings.Password), logger);
		});

		services.AddScoped<IPackageStorage, PackageStorage>();
		services.AddScoped(sp => sp.GetRequiredService<SftpClientWrapper>().CreateAccessor());

		return services;
	}
}