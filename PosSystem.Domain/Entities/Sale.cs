using System.ComponentModel.DataAnnotations;

namespace PosSystem.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }
    
    public Guid CustomerId { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal Tax { get; set; }
    
    public decimal FinalAmount { get; set; }
    
    public string PaymentMethod { get; set; } = string.Empty;
    
    public string Status { get; set; } = "Completed"; // Pending, Completed, Cancelled
    
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    public string? Notes { get; set; }
    
    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public class SaleItem
{
    public Guid Id { get; set; }
    
    public Guid SaleId { get; set; }
    
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}