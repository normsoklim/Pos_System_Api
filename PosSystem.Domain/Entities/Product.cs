
namespace  PosSystem.Domain.Entities;

public class  Product 
{
    public Guid Id { get; set; }

    public string ProductName { get; set; } = null!;

    public string Barcode { get; set; } = null!;

    public decimal Cost { get; set; }

    public decimal SellPrice { get; set; }

    public int QtyInStock { get; set; }

    public string Description { get; set; } = null!;

     public string? ImageUrl { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
