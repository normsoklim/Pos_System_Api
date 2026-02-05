namespace PosSystem.Application.DTOs.Role
{
    public class UserRoleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
    }
}