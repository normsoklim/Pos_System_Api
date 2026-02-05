using System.ComponentModel.DataAnnotations;

namespace PosSystem.Application.DTOs.Role
{
    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoleId { get; set; }
    }
}