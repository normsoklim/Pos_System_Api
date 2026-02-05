using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosSystem.Domain.Entities
{
    [Table("UserRoles")]
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int RoleId { get; set; }
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }
}