using System.ComponentModel.DataAnnotations;

namespace PosSystem.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public int UserId { get; set; }
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? RevokedAt { get; set; }
        
        public string? RevokedByIp { get; set; }
        
        public string? ReplacedByToken { get; set; }
        
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        
        public bool IsRevoked => RevokedAt != null;
        
        public bool IsActive => !IsExpired && !IsRevoked;
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}