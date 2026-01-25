
namespace  PosSystem.Domain.Entities;

public class  Product 
{
    public int Id { get; set; }

    public string ProductName { get; set; }

    public decimal Cost { get; set; }

    public decimal SellPrice { get; set; }

    public int QtyInStock { get; set; }

    public string Description { get; set; }

    public Category Category { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
