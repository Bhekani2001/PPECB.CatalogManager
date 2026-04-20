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
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<User?> GetUserWithRolesAsync(string email)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<User?> GetUserWithRefreshTokensAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted &&
                (excludeId == null || u.Id != excludeId));
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(u => u.Username == username && !u.IsDeleted &&
                (excludeId == null || u.Id != excludeId));
        }

        public async Task UpdateLastLoginDateAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                Update(user);
            }
        }

        public async Task IncrementFailedLoginAttemptsAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLockedOut = true;
                    user.LockoutEndDate = DateTime.UtcNow.AddMinutes(30);
                }
                Update(user);
            }
        }

        public async Task ResetFailedLoginAttemptsAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.FailedLoginAttempts = 0;
                user.IsLockedOut = false;
                user.LockoutEndDate = null;
                Update(user);
            }
        }

        public async Task LockUserAccountAsync(int userId, DateTime lockoutEndDate)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsLockedOut = true;
                user.LockoutEndDate = lockoutEndDate;
                Update(user);
            }
        }

        public async Task UnlockUserAccountAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsLockedOut = false;
                user.LockoutEndDate = null;
                user.FailedLoginAttempts = 0;
                Update(user);
            }
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash, string salt)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.PasswordHash = passwordHash;
                user.Salt = salt;
                Update(user);
            }
        }

        public async Task UpdateLastPasswordChangeDateAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastPasswordChangeDate = DateTime.UtcNow;
                Update(user);
            }
        }

        public async Task ConfirmEmailAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsEmailConfirmed = true;
                Update(user);
            }
        }

        public async Task AddUserToRoleAsync(int userId, int roleId)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedDate = DateTime.UtcNow
            };
            await _context.UserRoles.AddAsync(userRole);
        }

        public async Task RemoveUserFromRoleAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
            }
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<bool> IsUserInRoleAsync(int userId, string roleName)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken?> GetRefreshTokenByUserIdAsync(int userId)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                _context.RefreshTokens.Update(refreshToken);
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            _context.RefreshTokens.UpdateRange(tokens);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsActive && !u.IsLockedOut && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetLockedOutUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsLockedOut && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _dbSet
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName) && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(u => (u.Email.Contains(searchTerm) ||
                            u.Username.Contains(searchTerm) ||
                            u.FirstName.Contains(searchTerm) ||
                            u.LastName.Contains(searchTerm)) && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task UpdateUserProfileAsync(int userId, string firstName, string lastName, string? phoneNumber, string? mobileNumber)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.PhoneNumber = phoneNumber;
                user.MobileNumber = mobileNumber;
                Update(user);
            }
        }

        public async Task UpdateUserActiveStatusAsync(int userId, bool isActive)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                Update(user);
            }
        }

        public async Task UpdateUserProfileImageAsync(int userId, string profileImageUrl)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.ProfileImageUrl = profileImageUrl;
                Update(user);
            }
        }
    }
}