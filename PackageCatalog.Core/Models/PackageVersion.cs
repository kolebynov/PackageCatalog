using PackageCatalog.Core.Objects;

namespace PackageCatalog.Core.Models;

public class PackageVersion
{
	public StringId PackageId { get; }

	public Version Version { get; }

	public IReadOnlyDictionary<string, string>? AdditionalInfo { get; }

	public long Size { get; }

	public PackageVersion(StringId packageId, Version version, IReadOnlyDictionary<string, string>? additionalInfo,
		long size)
	{
		if (size == 0)
		{
			throw new ArgumentException("Size cannot be 0.", nameof(size));
		}

		PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
		Version = version ?? throw new ArgumentNullException(nameof(version));
		AdditionalInfo = additionalInfo;
		Size = size;
	}

	public override string ToString() => $"{PackageId} {Version}";
}