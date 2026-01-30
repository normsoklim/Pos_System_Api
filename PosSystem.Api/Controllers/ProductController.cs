
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Application.DTOs.Product;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PosSystem.Api.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize(Policy = "ManageProducts")]
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
    [Authorize(Policy = "ManageProducts")]
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
            QtyInStock = model.QtyInStock,
            Description = model.Description,
            ImageUrl = finalImageUrl,
            CategoryId = model.CategoryId
        };

        await _service.UpdateAsync(id, productDto);
        return Ok(new { message = "Product updated successfully" });
    }

    /// <summary>
    /// Partially update an existing product with JSON data
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="dto">Product partial update DTO with only the fields to update</param>
    /// <returns>Success message if product is updated</returns>
    [HttpPatch("{id}")]
    [Consumes("application/json")]
    [Authorize(Policy = "ManageProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Partially update an existing product with JSON",
        Description = "Partially updates an existing product using JSON data. Only provided fields will be updated."
    )]
    public async Task<IActionResult> UpdatePartialJson(Guid id, [FromBody] UpdateProductPartialDto dto)
    {
        await _service.UpdatePartialAsync(id, dto);
        return Ok(new { message = "Product updated successfully" });
    }

        /// <summary>
        /// Partially update an existing product with an optional image upload
        /// </summary>
        /// <param name="id">Product ID to update</param>
        /// <param name="model">Product partial update model with optional image</param>
        /// <returns>Success message if product is updated</returns>
        [HttpPatch("{id}/form")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = "ManageProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Partially update an existing product with form data",
            Description = "Partially updates an existing product with optional image upload. Only provided fields will be updated. Supports file upload via multipart/form-data."
        )]
        public async Task<IActionResult> UpdatePartialForm(Guid id, [FromForm] UpdateProductPartialModel model)
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

            // Create partial update DTO
            var productDto = new UpdateProductPartialDto
            {
                ProductName = model.ProductName,
                Barcode = model.Barcode,
                Cost = model.Cost,
                QtyInStock = model.QtyInStock,
                Description = model.Description,
                ImageUrl = finalImageUrl, // This can be null if neither image file nor imageUrl is provided
                CategoryId = model.CategoryId
            };

            try
            {
                await _service.UpdatePartialAsync(id, productDto);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Product not found" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ManageProducts")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok("Product deleted");
        }
    }


