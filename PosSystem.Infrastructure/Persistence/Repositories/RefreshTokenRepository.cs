using Microsoft.EntityFrameworkCore;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;
using PosSystem.Infrastructure.Persistence;

namespace PosSystem.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<bool> RevokeAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken == null) return false;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            return tokenEntity != null && tokenEntity.IsRevoked;
        }
    }
}