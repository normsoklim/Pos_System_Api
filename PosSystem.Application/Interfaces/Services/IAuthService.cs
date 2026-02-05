using PosSystem.Application.DTOs.Auth;

namespace PosSystem.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto> LoginAsync(LoginDto loginDto);
        Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}