
namespace PosSystem.Application.DTOs.Category;

public class UpdateCategoryDto
{
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
}