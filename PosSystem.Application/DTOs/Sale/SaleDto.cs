namespace PosSystem.Application.DTOs.Sale;

public class SaleDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; } 
    public decimal FinalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string? Notes { get; set; }
    public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
}

public class CreateSaleDto
{
    public Guid CustomerId { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateSaleItemDto> SaleItems { get; set; } = new List<CreateSaleItemDto>();
}

public class CreateSaleItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; } // Made nullable to allow auto-fetching
}

public class SaleItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}