using System.ComponentModel.DataAnnotations;

namespace PackageCatalog.Contracts.V1;

public class AddPackageVersionRequestV1
{
	[Required]
	public Version Version { get; init; } = null!;

	public IReadOnlyDictionary<string, string>? AdditionalData { get; init; }

	[Required]
	public string UploadedContentTicket { get; init; } = null!;
}