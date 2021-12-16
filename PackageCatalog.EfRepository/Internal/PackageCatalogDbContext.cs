using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.EfRepository.Internal;

internal class PackageCatalogDbContext : DbContext
{
	public DbSet<Category> Categories { get; set; } = null!;

	public DbSet<Package> Packages { get; set; } = null!;

	public DbSet<PackageVersion> PackageVersions { get; set; } = null!;

	public PackageCatalogDbContext(DbContextOptions<PackageCatalogDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		var stringIdConverter = new ValueConverter<StringId, string>(x => x.Value, x => new StringId(x));

		modelBuilder.Entity<Category>(x =>
		{
			x.ToTable(nameof(Category));
			x.HasKey(y => y.Id);
			x.Property(y => y.Id).HasConversion(stringIdConverter);
			x.Property(y => y.DisplayName);
		});

		modelBuilder.Entity<Package>(x =>
		{
			x.ToTable(nameof(Package));
			x.HasKey(y => y.Id);
			x.Property(y => y.Id).HasConversion(stringIdConverter);
			x.Property(y => y.CategoryId).HasConversion(stringIdConverter);
			x.HasOne(typeof(Category))
				.WithMany()
				.HasForeignKey(nameof(Package.CategoryId));
			x.Property(y => y.DisplayName);
		});

		modelBuilder.Entity<PackageVersion>(x =>
		{
			x.ToTable(nameof(PackageVersion));
			x.HasKey(y => new { y.PackageId, y.Version });
			x.HasOne(typeof(Package))
				.WithMany()
				.HasForeignKey(nameof(PackageVersion.PackageId));
			x.Property(y => y.PackageId).HasConversion(stringIdConverter);
			x.Property(y => y.AdditionalInfo)
				.HasConversion(
					y => JsonSerializer.Serialize(y, (JsonSerializerOptions?)null),
					y => !string.IsNullOrEmpty(y)
						? JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(y, (JsonSerializerOptions?)null)
						: null);
			x.Property(y => y.Version)
				.HasConversion(y => VersionToLong(y), y => LongToVersion(y));
			x.Property(y => y.Size);
		});
	}

	private static long VersionToLong(Version version)
	{
		return ((long)version.Major << 48) | ((long)version.Minor << 32 & 0xFFFF00000000)
		                                   | ((long)version.Build << 16 & 0xFFFF0000) | (long)version.Revision & 0xFFFF;
	}

	private static Version LongToVersion(long version)
	{
		var major = (short)((version >> 48) & 0xFFFF);
		var minor = (short)((version >> 32) & 0xFFFF);
		var build = (short)((version >> 16) & 0xFFFF);
		var revision = (short)(version & 0xFFFF);

		return (build, revision) switch
		{
			(-1, -1) => new Version(major, minor),
			(_, -1) => new Version(major, minor, build),
			_ => new Version(major, minor, build, revision),
		};
	}
}