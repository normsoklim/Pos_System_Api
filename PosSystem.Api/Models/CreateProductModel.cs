using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Models
{
    /// <summary>
    /// Model for creating a product with an optional image file
    /// </summary>
    public class CreateProductModel
    {
        /// <summary>
        /// Name of the product
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = null!;
        
        /// <summary>
        /// Barcode of the product
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Barcode { get; set; } = null!;
        
        /// <summary>
        /// Price of the product
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }
        
        
        
        /// <summary>
        /// Quantity in stock
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive value")]
        public int QtyInStock { get; set; }
        
        /// <summary>
        /// Description of the product
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = null!;
        
        /// <summary>
        /// Image file to upload (optional if ImageUrl is provided)
        /// </summary>
        public IFormFile? ImageFile { get; set; }
        
        /// <summary>
        /// URL of the image (optional if ImageFile is provided)
        /// </summary>
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Category ID for the product
        /// </summary>
        [Required]
        public Guid CategoryId { get; set; }
    }
}