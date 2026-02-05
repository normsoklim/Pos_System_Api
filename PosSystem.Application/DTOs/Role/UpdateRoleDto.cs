using System.ComponentModel.DataAnnotations;

namespace PosSystem.Application.DTOs.Role
{
    public class UpdateRoleDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        
        [MaxLength(255)]
        public string? Description { get; set; }
    }
}