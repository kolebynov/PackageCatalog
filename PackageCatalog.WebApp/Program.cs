using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PackageCatalog.Client.V1.Extensions;
using PackageCatalog.WebApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddPackageCatalogClientV1(opt => builder.Configuration.Bind("apiSettings", opt))
	.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());

await builder.Build().RunAsync();