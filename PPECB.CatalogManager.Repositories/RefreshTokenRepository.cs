using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<IEnumerable<RefreshToken>> GetUserRefreshTokensAsync(int userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetValidUserRefreshTokensAsync(int userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeTokenAsync(string token, string revokedByIp)
        {
            var refreshToken = await GetByTokenAsync(token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedByIp = revokedByIp;
                Update(refreshToken);
            }
        }

        public async Task RevokeAllUserTokensAsync(int userId, string revokedByIp)
        {
            var tokens = await GetUserRefreshTokensAsync(userId);
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedByIp = revokedByIp;
            }
            UpdateRange(tokens);
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow)
                .ToListAsync();
            _dbSet.RemoveRange(expiredTokens);
            return expiredTokens.Count;
        }

        public async Task<int> CleanupRevokedTokensAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldRevokedTokens = await _dbSet
                .Where(rt => rt.IsRevoked && rt.UpdatedAt <= cutoffDate)
                .ToListAsync();
            _dbSet.RemoveRange(oldRevokedTokens);
            return oldRevokedTokens.Count;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            return await _dbSet
                .AnyAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<RefreshToken?> GetTokenWithUserAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
    }
}