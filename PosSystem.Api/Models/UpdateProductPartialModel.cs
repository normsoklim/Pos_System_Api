using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace PosSystem.Api.Models
{
    /// <summary>
    /// Model for partially updating a product with an optional image file
    /// </summary>
    public class UpdateProductPartialModel
    {
        /// <summary>
        /// Name of the product (optional)
        /// </summary>
        [StringLength(100)]
        public string? ProductName { get; set; }
        
        /// <summary>
        /// Barcode of the product (optional)
        /// </summary>
        [StringLength(50)]
        public string? Barcode { get; set; }
        
        /// <summary>
        /// Cost of the product (optional)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive value")]
        public decimal? Cost { get; set; }
        
        
        /// <summary>
        /// Quantity in stock (optional)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive value")]
        public int? QtyInStock { get; set; }
        
        /// <summary>
        /// Description of the product (optional)
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Image file to upload (optional if ImageUrl is provided)
        /// </summary>
        [SwaggerSchema(Description = "Image file to upload (jpg, jpeg, png, webp)", Format = "binary")]
        public IFormFile? ImageFile { get; set; }
        
        /// <summary>
        /// URL of the image (optional if ImageFile is provided)
        /// </summary>
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Category ID for the product (optional)
        /// </summary>
        public Guid? CategoryId { get; set; }
    }
}