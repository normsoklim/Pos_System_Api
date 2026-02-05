using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Application.DTOs.Product;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PosSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using PosSystem.Api.Services;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IWebHostEnvironment _env;
    private readonly IFileService _fileService;

    public ProductController(IProductService service, IWebHostEnvironment env, IFileService fileService)
    {
        _service = service;
        _env = env;
        _fileService = fileService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var product = await _service.GetByIdAsync(id);
            return Ok(product);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Product not found"))
            {
                return NotFound(new { message = "Product not found" });
            }
            return BadRequest(new { message = "Error retrieving product: " + ex.Message });
        }
    }
    [HttpPost]
    [Authorize(Policy = "CanCreateProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateProductModel model)
    {
        string? finalImageUrl = null;

        // Handle image upload if provided
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                // Read file data to byte array
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                // Use the file service to save the file
                finalImageUrl = await _fileService.SaveFileAsync(fileData, model.ImageFile.FileName, "images/products");
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                // For now, return a generic error to avoid exposing system details
                return BadRequest("Error uploading image file");
            }
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
            Price = model.Price,
            QtyInStock = model.QtyInStock,
            Description = model.Description,
            ImageUrl = finalImageUrl,
            CategoryId = model.CategoryId
        };

        try
        {
            await _service.CreateAsync(productDto);
            return Ok(new { message = "Product created successfully" });
        }
        catch (Exception ex)
        {
            // Handle specific service exceptions
            if (ex.Message.Contains("Category") && ex.Message.Contains("does not exist"))
            {
                return BadRequest(new { message = ex.Message });
            }
            return BadRequest(new { message = "Error creating product: " + ex.Message });
        }
    }
    [Authorize(Policy = "CanUpdateProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductModel model)
    {
        string? finalImageUrl = null;

        // Handle image upload if provided
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                // Read file data to byte array
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                // Use the file service to save the file
                finalImageUrl = await _fileService.SaveFileAsync(fileData, model.ImageFile.FileName, "images/products");
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                // For now, return a generic error to avoid exposing system details
                return BadRequest("Error uploading image file");
            }
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
            Price = model.Price,
            QtyInStock = model.QtyInStock,
            Description = model.Description,
            ImageUrl = finalImageUrl,
            CategoryId = model.CategoryId
        };

        try
        {
            await _service.UpdateAsync(id, productDto);
            return Ok(new { message = "Product updated successfully" });
        }
        catch (Exception ex)
        {
            // Handle specific service exceptions
            if (ex.Message.Contains("Product not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            if (ex.Message.Contains("Category") && ex.Message.Contains("does not exist"))
            {
                return BadRequest(new { message = ex.Message });
            }
            return BadRequest(new { message = "Error updating product: " + ex.Message });
        }
    }
    [HttpPatch("{id}")]
    [Authorize(Policy = "CanUpdateProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartialJson(Guid id)
    {
        // Check content type to determine how to process the request
        if (Request.ContentType?.StartsWith("application/json") == true)
        {
            // Handle JSON request
            var dto = await System.Text.Json.JsonSerializer.DeserializeAsync<UpdateProductPartialDto>(Request.Body);
            try
            {
                await _service.UpdatePartialAsync(id, dto);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Product not found" });
            }
            catch (Exception ex)
            {
                // Handle specific service exceptions
                if (ex.Message.Contains("Product not found"))
                {
                    return NotFound(new { message = ex.Message });
                }
                if (ex.Message.Contains("Category") && ex.Message.Contains("does not exist"))
                {
                    return BadRequest(new { message = ex.Message });
                }
                return BadRequest(new { message = "Error updating product: " + ex.Message });
            }
        }
        else
        {
            // For non-JSON content (like form data), redirect to the form endpoint
            // This helps guide clients to use the correct endpoint
            return BadRequest(new {
                message = "For form data or file uploads, please use the /api/product/{id}/form endpoint",
                suggestedEndpoint = $"/api/product/{id}/form"
            });
        }
    }

    [HttpPatch("{id}/form")]
    [Authorize(Policy = "CanUpdateProduct")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartialForm(Guid id, [FromForm] UpdateProductPartialModel model)
    {
        string? finalImageUrl = null;

        // Handle image upload if provided
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                // Read file data to byte array
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                // Use the file service to save the file
                finalImageUrl = await _fileService.SaveFileAsync(fileData, model.ImageFile.FileName, "images/products");
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                // For now, return a generic error to avoid exposing system details
                return BadRequest("Error uploading image file");
            }
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
            Price = model.Price,
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
        catch (Exception ex)
        {
            // Handle specific service exceptions
            if (ex.Message.Contains("Product not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            if (ex.Message.Contains("Category") && ex.Message.Contains("does not exist"))
            {
                return BadRequest(new { message = ex.Message });
            }
            return BadRequest(new { message = "Error updating product: " + ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteProduct")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Product not found" });
        }
        catch (Exception ex)
        {
            // Log the exception (in a real app, use proper logging)
            return BadRequest(new { message = "Error deleting product" });
        }
    }
}
