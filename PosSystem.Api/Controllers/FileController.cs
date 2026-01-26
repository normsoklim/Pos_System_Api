
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FileController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("upload-product-image")]
    public async Task<IActionResult> UploadProductImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // ✅ Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (Array.IndexOf(allowedExtensions, extension) < 0)
            return BadRequest("Invalid image format");

        // ✅ Ensure folder exists
        var folderPath = Path.Combine(_env.WebRootPath, "images", "products");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // ✅ Unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        // ✅ Save file
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // ✅ Return URL
        var imageUrl = $"/images/products/{fileName}";

        return Ok(new
        {
            imageUrl
        });
    }
}