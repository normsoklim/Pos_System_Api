
namespace PosSystem.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}