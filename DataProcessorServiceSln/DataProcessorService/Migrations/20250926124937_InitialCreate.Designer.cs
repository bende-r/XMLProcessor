using System;
using DataProcessorService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataProcessorService.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250926124937_InitialCreate")]
    partial class InitialCreate
    {
                protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("DataProcessorService.Data.ModuleStatus", b =>
                {
                    b.Property<string>("ModuleCategoryID")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModuleState")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ModuleCategoryID");

                    b.ToTable("ModuleStatuses");
                });
#pragma warning restore 612, 618
        }
    }
}
