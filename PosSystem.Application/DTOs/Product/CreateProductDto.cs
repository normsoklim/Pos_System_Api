
using System.Text.Json.Serialization;

namespace PosSystem.Application.DTOs.Product
{
    public class CreateProductDto
    {
        public string ProductName { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public decimal Price { get; set; }
        
        public int QtyInStock { get; set; }
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public Guid CategoryId { get; set; }
    }
}