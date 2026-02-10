
namespace PosSystem.Application.DTOs.Stock
{
    public class CreateStockDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
    }
}