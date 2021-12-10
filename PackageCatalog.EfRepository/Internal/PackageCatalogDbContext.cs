using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using PackageCatalog.Core.Models;

namespace PackageCatalog.EfRepository.Internal;

public class PackageCatalogDbContext : DbContext
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

		modelBuilder.Entity<Category>(x =>
		{
			x.ToTable(nameof(Category));
			x.HasKey(y => y.Id);
			x.Property(y => y.DisplayName);
		});

		modelBuilder.Entity<Package>(x =>
		{
			x.ToTable(nameof(Package));
			x.HasKey(y => y.Id);
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
			x.Property(y => y.AdditionalInfo)
				.HasConversion(
					y => JsonSerializer.Serialize(y, (JsonSerializerOptions?)null),
					y => !string.IsNullOrEmpty(y)
						? JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(y, (JsonSerializerOptions?)null)
						: null);
			x.Property(y => y.Version)
				.HasConversion(y => y.ToString(), y => NuGetVersion.Parse(y));
			x.Property(y => y.Checksum);
			x.Property(y => y.Size);
		});
	}
}