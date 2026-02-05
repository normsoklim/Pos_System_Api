using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosSystem.Domain.Entities
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = null!;
        
        [Required]
        public int UserId { get; set; }
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsRevoked { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? RevokedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}