
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Application.DTOs.Product;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PosSystem.Api.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }
    
        /// <summary>
        /// Create a new product with an optional image upload
        /// </summary>
        /// <param name="model">Product creation model with optional image</param>
        /// <returns>Success message if product is created</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Create a new product",
            Description = "Creates a new product with optional image upload. Supports file upload via multipart/form-data."
        )]
        public async Task<IActionResult> Create([FromForm] CreateProductModel model)
        {
            string? finalImageUrl = null;
            
            // Handle image upload if provided
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                if (Array.IndexOf(allowedExtensions, extension) < 0)
                    return BadRequest("Invalid image format");

                // Ensure folder exists
                var webRootPath = Directory.GetCurrentDirectory();
                var folderPath = Path.Combine(webRootPath, "wwwroot", "images", "products");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Create unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                // Save file
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                // Create image URL
                finalImageUrl = $"/images/products/{fileName}";
            }
            else
            {
                // Use the imageUrl parameter if no file was uploaded
                finalImageUrl = model.ImageUrl;
            }

            // Create product DTO
            var productDto = new CreateProductDto
            {
                ProductName = model.ProductName,
                Barcode = model.Barcode,
                Cost = model.Cost,
                SellPrice = model.SellPrice,
                QtyInStock = model.QtyInStock,
                Description = model.Description,
                ImageUrl = finalImageUrl,
                CategoryId = model.CategoryId
            };

            await _service.CreateAsync(productDto);
            return Ok(new { message = "Product created successfully" });
        }

    /// <summary>
    /// Update an existing product with an optional image upload
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="model">Product update model with optional image</param>
    /// <returns>Success message if product is updated</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Update an existing product",
        Description = "Updates an existing product with optional image upload. Supports file upload via multipart/form-data."
    )]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductModel model)
    {
        string? finalImageUrl = null;
        
        // Handle image upload if provided
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

            if (Array.IndexOf(allowedExtensions, extension) < 0)
                return BadRequest("Invalid image format");

            // Ensure folder exists
            var webRootPath = Directory.GetCurrentDirectory();
            var folderPath = Path.Combine(webRootPath, "wwwroot", "images", "products");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ImageFile.CopyToAsync(stream);

            // Create image URL
            finalImageUrl = $"/images/products/{fileName}";
        }
        else
        {
            // Use the imageUrl parameter if no file was uploaded
            finalImageUrl = model.ImageUrl;
        }

        // Create product DTO
        var productDto = new UpdateProductDto
        {
            ProductName = model.ProductName,
            Barcode = model.Barcode,
            Cost = model.Cost,
            SellPrice = model.SellPrice,
            QtyInStock = model.QtyInStock,
            Description = model.Description,
            ImageUrl = finalImageUrl,
            CategoryId = model.CategoryId
        };

        await _service.UpdateAsync(id, productDto);
        return Ok(new { message = "Product updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok("Product deleted");
    }
}

