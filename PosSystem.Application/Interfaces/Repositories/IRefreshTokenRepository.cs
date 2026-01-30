using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetValidTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
        Task CleanExpiredTokensAsync();
    }
}