using PosSystem.Application.DTOs.Auth;

namespace PosSystem.Application.Interfaces.Services;

public interface IAuthService
{
    Task<TokenResponseDto> LoginAsync(LoginDto loginDto);
    Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task<bool> UserHasRoleAsync(int userId, string roleName);
}