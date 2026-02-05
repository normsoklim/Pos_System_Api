using PosSystem.Application.DTOs.Role;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(MapToRoleDto).ToList();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return role != null ? MapToRoleDto(role) : null;
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string name)
        {
            var role = await _roleRepository.GetByNameAsync(name);
            return role != null ? MapToRoleDto(role) : null;
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                CreatedAt = DateTime.UtcNow
            };
            
            var createdRole = await _roleRepository.CreateAsync(role);
            return MapToRoleDto(createdRole);
        }

        public async Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto updateRoleDto)
        {
            var existingRole = await _roleRepository.GetByIdAsync(id);
            if (existingRole == null)
                throw new ArgumentException($"Role with ID {id} not found");

            existingRole.Name = updateRoleDto.Name;
            existingRole.Description = updateRoleDto.Description;
            
            var updatedRole = await _roleRepository.UpdateAsync(existingRole);
            return MapToRoleDto(updatedRole);
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            return await _roleRepository.DeleteAsync(id);
        }

        public async Task<bool> AssignRoleToUserAsync(AssignRoleDto assignRoleDto)
        {
            var user = await _userRepository.GetByIdAsync(assignRoleDto.UserId);
            var role = await _roleRepository.GetByIdAsync(assignRoleDto.RoleId);

            if (user == null || role == null)
                return false;

            // Check if the user already has this role
            var existingUserRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == assignRoleDto.RoleId);
            if (existingUserRole != null)
                return false; // User already has this role

            var userRole = new UserRole
            {
                UserId = assignRoleDto.UserId,
                RoleId = assignRoleDto.RoleId,
                AssignedAt = DateTime.UtcNow
            };

            user.UserRoles.Add(userRole);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var user = await _userRepository.GetWithRolesAsync(userId);
            if (user == null)
                return false;

            var userRole = user.UserRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null)
                return false;

            user.UserRoles.Remove(userRole);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId)
        {
            var user = await _userRepository.GetWithRolesAsync(userId);
            if (user == null)
                return new List<UserRoleDto>();

            return user.UserRoles.Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAt = ur.AssignedAt
            }).ToList();
        }

        public async Task<IEnumerable<int>> GetUserRoleIdsAsync(int userId)
        {
            var user = await _userRepository.GetWithRolesAsync(userId);
            if (user == null)
                return new List<int>();

            return user.UserRoles.Select(ur => ur.RoleId).ToList();
        }

        private static RoleDto MapToRoleDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            };
        }
    }
}