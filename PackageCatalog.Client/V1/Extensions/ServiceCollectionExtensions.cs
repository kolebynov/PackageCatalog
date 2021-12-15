using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PackageCatalog.Client.Configuration;
using PackageCatalog.Client.V1.Interfaces;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace PackageCatalog.Client.V1.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPackageCatalogClientV1(
		this IServiceCollection services, Action<ClientSettings> settingsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.AddRefitClient<IPackageCatalogV1>()
			.ConfigureHttpClient((sp, httpClient) =>
			{
				var settings = sp.GetRequiredService<IOptions<ClientSettings>>().Value;
				httpClient.BaseAddress = settings.BaseUri;
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
					"Bearer", settings.AccessToken);
			})
			.AddPolicyHandler(GetRetryPolicy());

		return services;
	}

	private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
		HttpPolicyExtensions
			.HandleTransientHttpError()
			.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}