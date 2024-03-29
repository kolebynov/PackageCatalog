﻿using System.Net;
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
	public static IHttpClientBuilder AddPackageCatalogClientV1(
		this IServiceCollection services, Action<ClientSettings> settingsAction)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.AddSharedPackageServices();
		var httpBuilder = services.AddRefitClient<ILowLevelClientV1>()
			.ConfigureHttpClient((sp, httpClient) =>
			{
				var settings = sp.GetRequiredService<IOptions<ClientSettings>>().Value;
				httpClient.BaseAddress = settings.BaseUri;
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
					"Bearer", settings.AccessToken);
				httpClient.Timeout = TimeSpan.FromMinutes(10);
			})
			.AddPolicyHandler(GetRetryPolicy())
			.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler());

		services.Configure(settingsAction);

		services.AddTransient<IPackageCatalogClientV1, PackageCatalogClientV1>();

		return httpBuilder;
	}

	private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
		Policy<HttpResponseMessage>
			.Handle<HttpRequestException>()
			.OrResult(r => r.StatusCode is > HttpStatusCode.NotImplemented or HttpStatusCode.RequestTimeout)
			.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}