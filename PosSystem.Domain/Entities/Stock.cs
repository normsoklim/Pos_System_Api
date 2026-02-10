

namespace PosSystem.Domain.Entities
{
    public class Stock
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;    

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}