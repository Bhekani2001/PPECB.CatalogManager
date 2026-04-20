using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IUserBusinessLogic
    {
        // Authentication
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress, string userAgent);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken, string ipAddress);
        Task<bool> LogoutAsync(int userId, string token);

        // Password management
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        // User management
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> UpdateUserProfileAsync(int userId, UpdateProfileDto updateDto);
        Task<bool> UpdateUserProfileImageAsync(int userId, string imageUrl);

        // Role management (Admin only)
        Task<bool> AssignRoleToUserAsync(int userId, string roleName, string assignedBy);
        Task<bool> RemoveRoleFromUserAsync(int userId, string roleName);
        Task<bool> ActivateUserAsync(int userId, string activatedBy);
        Task<bool> DeactivateUserAsync(int userId, string deactivatedBy);
        Task<bool> LockUserAsync(int userId, int minutes, string lockedBy);
        Task<bool> UnlockUserAsync(int userId, string unlockedBy);

        // Validation
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);

        // Get all users
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetActiveUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName);
    }
}