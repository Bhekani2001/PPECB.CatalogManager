using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        // Authentication
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetUserWithRolesAsync(string email);
        Task<User?> GetUserWithRefreshTokensAsync(int userId);

        // Account management
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null);

        // Login tracking
        Task UpdateLastLoginDateAsync(int userId);
        Task IncrementFailedLoginAttemptsAsync(int userId);
        Task ResetFailedLoginAttemptsAsync(int userId);
        Task LockUserAccountAsync(int userId, DateTime lockoutEndDate);
        Task UnlockUserAccountAsync(int userId);

        // Password management
        Task UpdatePasswordAsync(int userId, string passwordHash, string salt);
        Task UpdateLastPasswordChangeDateAsync(int userId);

        // Email confirmation
        Task ConfirmEmailAsync(int userId);

        // Role management
        Task AddUserToRoleAsync(int userId, int roleId);
        Task RemoveUserFromRoleAsync(int userId, int roleId);
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
        Task<bool> IsUserInRoleAsync(int userId, string roleName);

        // Refresh tokens
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken?> GetRefreshTokenByUserIdAsync(int userId);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokensAsync(int userId);

        // Active users
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetLockedOutUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);

        // Search
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);

        // Profile updates
        Task UpdateUserProfileAsync(int userId, string firstName, string lastName, string? phoneNumber, string? mobileNumber);
        Task UpdateUserActiveStatusAsync(int userId, bool isActive);
        Task UpdateUserProfileImageAsync(int userId, string profileImageUrl);
    }
}