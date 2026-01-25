
namespace PosSystem.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
}