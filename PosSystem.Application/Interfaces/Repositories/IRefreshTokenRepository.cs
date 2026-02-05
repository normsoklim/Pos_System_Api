using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<bool> RevokeAsync(string token);
        Task<bool> IsTokenRevokedAsync(string token);
    }
}