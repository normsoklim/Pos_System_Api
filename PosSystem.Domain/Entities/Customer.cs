
namespace PosSystem.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address {get; set; } =null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}