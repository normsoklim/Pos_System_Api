using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.DTOs.Role;
using PosSystem.Application.Interfaces.Services;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound($"Role with ID {id} not found");

            return Ok(role);
        }

        [HttpGet("name/{name}")]
        [Authorize]
        public async Task<ActionResult<RoleDto>> GetRoleByName(string name)
        {
            var role = await _roleService.GetRoleByNameAsync(name);
            if (role == null)
                return NotFound($"Role with name '{name}' not found");

            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createRoleDto)
        {
            var role = await _roleService.CreateRoleAsync(createRoleDto);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<RoleDto>> UpdateRole(int id, UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _roleService.UpdateRoleAsync(id, updateRoleDto);
                return Ok(role);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
                return NotFound($"Role with ID {id} not found");

            return NoContent();
        }

        [HttpPost("assign")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> AssignRoleToUser(AssignRoleDto assignRoleDto)
        {
            var result = await _roleService.AssignRoleToUserAsync(assignRoleDto);
            if (!result)
                return BadRequest("Failed to assign role to user. User or role may not exist, or user may already have this role.");

            return Ok("Role assigned successfully.");
        }

        [HttpDelete("remove")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> RemoveRoleFromUser([FromQuery] int userId, [FromQuery] int roleId)
        {
            var result = await _roleService.RemoveRoleFromUserAsync(userId, roleId);
            if (!result)
                return BadRequest("Failed to remove role from user. User or role may not exist, or user may not have this role.");

            return Ok("Role removed successfully.");
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles(int userId)
        {
            var userRoles = await _roleService.GetUserRolesAsync(userId);
            return Ok(userRoles);
        }
    }
}