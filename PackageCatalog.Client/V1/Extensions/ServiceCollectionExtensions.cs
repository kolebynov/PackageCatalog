using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PackageCatalog.Client.Configuration;
using PackageCatalog.Client.V1.Interfaces;
using PackageCatalog.Client.V1.Internal;
using PackageCatalog.Shared.Extensions;
using Polly;
using Refit;

namespace PackageCatalog.Client.V1.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPackageCatalogClientV1(
		this IServiceCollection services, Action<ClientSettings> settingsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.AddSharedPackageServices();
		services.AddRefitClient<ILowLevelClientV1>()
			.ConfigureHttpClient((sp, httpClient) =>
			{
				var settings = sp.GetRequiredService<IOptions<ClientSettings>>().Value;
				httpClient.BaseAddress = settings.BaseUri;
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
					"Bearer", settings.AccessToken);
			})
			.AddPolicyHandler(GetRetryPolicy())
			.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler());

		services.Configure(settingsAction);

		services.AddTransient<IPackageCatalogClientV1, PackageCatalogClientV1>();

		return services;
	}

	private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
		Policy<HttpResponseMessage>
			.Handle<HttpRequestException>()
			.OrResult(r => r.StatusCode is > HttpStatusCode.NotImplemented or HttpStatusCode.RequestTimeout)
			.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}