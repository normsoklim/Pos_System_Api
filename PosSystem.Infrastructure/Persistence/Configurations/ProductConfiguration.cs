using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSystem.Domain.Entities;

namespace PosSystem.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd(); // Using Guid with ValueGeneratedOnAdd
        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Barcode).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Configure the relationship with Category
        builder.HasOne(p => p.Category)
               .WithMany()
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}