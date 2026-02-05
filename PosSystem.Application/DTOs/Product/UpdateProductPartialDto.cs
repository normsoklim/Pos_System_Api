using System.Text.Json.Serialization;

namespace PosSystem.Application.DTOs.Product
{
    public class UpdateProductPartialDto
    {
        public string? ProductName { get; set; }
        public string? Barcode { get; set; }
        public decimal? Price { get; set; }
        public int? QtyInStock { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
    }
}