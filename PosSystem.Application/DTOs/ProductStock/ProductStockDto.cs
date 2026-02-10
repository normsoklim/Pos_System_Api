namespace PosSystem.Application.DTOs.ProductStock;

public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ProductQtyInStock { get; set; } // Qty from Product table
    public int StockQuantity { get; set; } // Qty from Stock table
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime LastUpdated { get; set; }
}