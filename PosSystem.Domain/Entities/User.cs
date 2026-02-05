using System.ComponentModel.DataAnnotations;

namespace PosSystem.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = null!;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
        
        [MaxLength(50)]
        public string? FirstName { get; set; }
        
        [MaxLength(50)]
        public string? LastName { get; set; }
        
        public string? ApiKey { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}