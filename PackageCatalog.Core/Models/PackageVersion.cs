using NuGet.Versioning;

namespace PackageCatalog.Core.Models;

public class PackageVersion
{
	public string PackageId { get; }

	public NuGetVersion Version { get; }

	public IReadOnlyDictionary<string, string>? AdditionalInfo { get; }

	public long Size { get; }

	public uint Checksum { get; }

	public PackageVersion(string packageId, NuGetVersion version, IReadOnlyDictionary<string, string>? additionalInfo,
		uint checksum, long size)
	{
		if (string.IsNullOrEmpty(packageId))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(packageId));
		}

		if (checksum == 0)
		{
			throw new ArgumentException("Checksum cannot be 0.", nameof(checksum));
		}

		if (size == 0)
		{
			throw new ArgumentException("Size cannot be 0.", nameof(size));
		}

		PackageId = packageId;
		Version = version ?? throw new ArgumentNullException(nameof(version));
		AdditionalInfo = additionalInfo;
		Checksum = checksum;
		Size = size;
	}

	public override string ToString() => $"{PackageId} {Version}";
}