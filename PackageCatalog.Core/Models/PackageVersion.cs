using NuGet.Versioning;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Models;

public class PackageVersion
{
	public StringId PackageId { get; }

	public NuGetVersion Version { get; }

	public IReadOnlyDictionary<string, string>? AdditionalInfo { get; }

	public long Size { get; }

	public uint Checksum { get; }

	public PackageVersion(StringId packageId, NuGetVersion version, IReadOnlyDictionary<string, string>? additionalInfo,
		uint checksum, long size)
	{
		if (checksum == 0)
		{
			throw new ArgumentException("Checksum cannot be 0.", nameof(checksum));
		}

		if (size == 0)
		{
			throw new ArgumentException("Size cannot be 0.", nameof(size));
		}

		PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
		Version = version ?? throw new ArgumentNullException(nameof(version));
		AdditionalInfo = additionalInfo;
		Checksum = checksum;
		Size = size;
	}

	public override string ToString() => $"{PackageId} {Version}";
}