using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class UserBusinessLogic : IUserBusinessLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UserBusinessLogic(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress, string userAgent)
        {
            var user = await _userRepository.GetUserWithRolesAsync(loginDto.Email);

            if (user == null)
            {
                await LogFailedAttempt(null, loginDto.Email, ipAddress, userAgent);
                return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto { Success = false, Message = "Account is deactivated" };
            }

            if (user.IsLockedOut && user.LockoutEndDate > DateTime.UtcNow)
            {
                return new AuthResponseDto { Success = false, Message = $"Account locked until {user.LockoutEndDate}" };
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash, user.Salt))
            {
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id);
                await LogFailedAttempt(user.Id, loginDto.Email, ipAddress, userAgent);
                return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
            }

            await _userRepository.ResetFailedLoginAttemptsAsync(user.Id);
            await _userRepository.UpdateLastLoginDateAsync(user.Id);

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress,
                DeviceInfo = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            await LogSuccess(user.Id, "Login", ipAddress, userAgent);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                Email = user.Email,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "User",
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress)
        {
            if (!await IsEmailUniqueAsync(registerDto.Email))
            {
                return new AuthResponseDto { Success = false, Message = "Email already registered" };
            }

            if (!await IsUsernameUniqueAsync(registerDto.Username))
            {
                return new AuthResponseDto { Success = false, Message = "Username already taken" };
            }

            var salt = GenerateSalt();
            var passwordHash = HashPassword(registerDto.Password, salt);

            var user = new User
            {
                Email = registerDto.Email,
                Username = registerDto.Username,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                MobileNumber = registerDto.MobileNumber,
                PasswordHash = passwordHash,
                Salt = salt,
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Self"
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful. Please login.",
                Email = user.Email,
                Username = user.Username
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken, string ipAddress)
        {
            var refreshTokenEntity = await _refreshTokenRepository.GetTokenWithUserAsync(refreshToken);

            if (refreshTokenEntity == null || refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token" };
            }

            var user = refreshTokenEntity.User;
            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            refreshTokenEntity.IsRevoked = true;
            _refreshTokenRepository.Update(refreshTokenEntity);

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                Email = user.Email,
                Username = user.Username,
                FullName = user.FullName
            };
        }

        public async Task<bool> LogoutAsync(int userId, string token)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "Logout");
            await _refreshTokenRepository.SaveChangesAsync();
            await LogSuccess(userId, "Logout", "", "");
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash, user.Salt))
            {
                return false;
            }

            var newSalt = GenerateSalt();
            var newPasswordHash = HashPassword(changePasswordDto.NewPassword, newSalt);

            await _userRepository.UpdatePasswordAsync(userId, newPasswordHash, newSalt);
            await _userRepository.UpdateLastPasswordChangeDateAsync(userId);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;

            // Generate reset token (simplified - in production use proper token)
            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // In production, send email with reset link
            // For now, just return true

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // Implement password reset logic
            // This would verify token and update password
            await Task.CompletedTask;
            return true;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto> UpdateUserProfileAsync(int userId, UpdateProfileDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found");

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.MobileNumber = updateDto.MobileNumber;
            user.Bio = updateDto.Bio;
            user.Department = updateDto.Department;
            user.JobTitle = updateDto.JobTitle;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserProfileImageAsync(int userId, string imageUrl)
        {
            await _userRepository.UpdateUserProfileImageAsync(userId, imageUrl);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, string roleName, string assignedBy)
        {
            // This would need a role repository
            // Simplified version
            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, string roleName)
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> ActivateUserAsync(int userId, string activatedBy)
        {
            await _userRepository.UpdateUserActiveStatusAsync(userId, true);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId, string deactivatedBy)
        {
            await _userRepository.UpdateUserActiveStatusAsync(userId, false);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LockUserAsync(int userId, int minutes, string lockedBy)
        {
            await _userRepository.LockUserAccountAsync(userId, DateTime.UtcNow.AddMinutes(minutes));
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(int userId, string unlockedBy)
        {
            await _userRepository.UnlockUserAccountAsync(userId);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            return await _userRepository.IsEmailUniqueAsync(email, excludeUserId);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
        {
            return await _userRepository.IsUsernameUniqueAsync(username, excludeUserId);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userRepository.GetUsersByRoleAsync(roleName);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName)
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role?.Name ?? "User"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyHereAtLeast32CharactersLong!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: "PPECB",
                audience: "PPECB",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash, string salt)
        {
            var hash = HashPassword(password, salt);
            return hash == storedHash;
        }

        private async Task LogSuccess(int userId, string action, string ipAddress, string userAgent)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = "User",
                EntityId = userId.ToString(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();
        }

        private async Task LogFailedAttempt(int? userId, string email, string ipAddress, string userAgent)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "FailedLogin",
                EntityName = "User",
                EntityId = email,
                OldValues = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();
        }
    }
}