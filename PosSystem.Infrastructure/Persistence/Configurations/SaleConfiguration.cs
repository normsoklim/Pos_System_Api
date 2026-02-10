using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSystem.Domain.Entities;

namespace PosSystem.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        // Table name
        builder.ToTable("Sales");
        
        // Primary key
        builder.HasKey(s => s.Id);
        
        // Properties
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(s => s.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(s => s.Discount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);
            
        builder.Property(s => s.Tax)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);
            
        builder.Property(s => s.FinalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(s => s.PaymentMethod)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(s => s.Status)
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(s => s.SaleDate)
            .IsRequired();
            
        builder.Property(s => s.Notes)
            .HasMaxLength(500);
        
        // Relationships
        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(s => s.SaleItems)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        // Table name
        builder.ToTable("SaleItems");
        
        // Primary key
        builder.HasKey(si => si.Id);
        
        // Properties
        builder.Property(si => si.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(si => si.Quantity)
            .IsRequired();
            
        builder.Property(si => si.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(si => si.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        // Relationships
        builder.HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}