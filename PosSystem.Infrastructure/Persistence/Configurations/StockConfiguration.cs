using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSystem.Domain.Entities;

namespace PosSystem.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        // Table name
        builder.ToTable("Stocks");
        
        // Primary key
        builder.HasKey(s => s.Id);
        
        // Properties
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(s => s.Quantity)
            .IsRequired();
            
        builder.Property(s => s.CostPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(s => s.SellPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(s => s.LastUpdated)
            .IsRequired();
            
        // Relationships
        builder.HasOne(s => s.Product)
            .WithMany() // Product entity doesn't have a collection of Stocks
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}