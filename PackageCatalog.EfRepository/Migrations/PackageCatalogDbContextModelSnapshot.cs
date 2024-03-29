﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PackageCatalog.EfRepository.Internal;

#nullable disable

namespace PackageCatalog.EfRepository.Migrations
{
    [DbContext(typeof(PackageCatalogDbContext))]
    partial class PackageCatalogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("PackageCatalog.Core.Models.Category", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Category", (string)null);
                });

            modelBuilder.Entity("PackageCatalog.Core.Models.Package", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Package", (string)null);
                });

            modelBuilder.Entity("PackageCatalog.Core.Models.PackageVersion", b =>
                {
                    b.Property<string>("PackageId")
                        .HasColumnType("TEXT");

                    b.Property<long>("Version")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AdditionalInfo")
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.HasKey("PackageId", "Version");

                    b.ToTable("PackageVersion", (string)null);
                });

            modelBuilder.Entity("PackageCatalog.Core.Models.Package", b =>
                {
                    b.HasOne("PackageCatalog.Core.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PackageCatalog.Core.Models.PackageVersion", b =>
                {
                    b.HasOne("PackageCatalog.Core.Models.Package", null)
                        .WithMany()
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
