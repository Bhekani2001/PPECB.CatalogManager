using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetValidTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetUserRefreshTokensAsync(int userId);
        Task<IEnumerable<RefreshToken>> GetValidUserRefreshTokensAsync(int userId);
        Task RevokeTokenAsync(string token, string revokedByIp);
        Task RevokeAllUserTokensAsync(int userId, string revokedByIp);
        Task<int> CleanupExpiredTokensAsync();
        Task<int> CleanupRevokedTokensAsync(int daysToKeep = 30);
        Task<bool> IsTokenValidAsync(string token);
        Task<RefreshToken?> GetTokenWithUserAsync(string token);
    }
}