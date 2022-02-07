using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackageCatalog.Client.V1.Extensions;
using PackageCatalog.Client.V1.Interfaces;

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json")
	.Build();
var services = new ServiceCollection();

services.AddPackageCatalogClientV1(opt => configuration.Bind(opt));

var sp = services.BuildServiceProvider();

var client = sp.GetRequiredService<IPackageCatalogClientV1>();
var low = sp.GetRequiredService<ILowLevelClientV1>();

// var categories = await client.GetCategories(CancellationToken.None).ToArrayAsync(CancellationToken.None);
// foreach (var category in categories)
// {
// Console.WriteLine(category.Id);
// }

var tasks = new List<Task<string>>();
for (int i = 0; i < 6; i++)
{
	tasks.Add(GetContent(low, i));
}

await Task.WhenAll(tasks);
foreach (var task in tasks)
{
	Console.WriteLine((await task).Substring(0, 16));
}

static async Task<string> GetContent(ILowLevelClientV1 client, int number)
{
	Console.WriteLine($"{DateTimeOffset.Now:O} #{number} Request executing");
	using var reader = new StreamReader(await client.GetPackageVersionContent("MicrosoftConnector", new Version("1.3.11"), CancellationToken.None));
	Console.WriteLine($"{DateTimeOffset.Now:O} #{number} Request executed");
	var content = await reader.ReadToEndAsync();
	Console.WriteLine($"{DateTimeOffset.Now:O} #{number} Content read");
	return content;
}