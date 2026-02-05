using PosSystem.Application.DTOs.Role;

namespace PosSystem.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto?> GetRoleByNameAsync(string name);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
        Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto updateRoleDto);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> AssignRoleToUserAsync(AssignRoleDto assignRoleDto);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId);
        Task<IEnumerable<int>> GetUserRoleIdsAsync(int userId);
    }
}