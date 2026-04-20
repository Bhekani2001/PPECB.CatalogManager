using System;
using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class User : BaseEntity
    {
        // Basic Info
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;

        // Personal Info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? MobileNumber { get; set; }

        // Address
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // Profile
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }

        // Account Status
        public bool IsActive { get; set; } = true;
        public bool IsEmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEndDate { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }

        // 2FA
        public bool IsTwoFactorEnabled { get; set; }
        public string? TwoFactorSecretKey { get; set; }

        // Refresh Tokens for JWT
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        // User Roles (Many-to-Many)
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Audit Trail (Actions performed by this user)
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Products created/updated by this user
        public virtual ICollection<Product> CreatedProducts { get; set; } = new List<Product>();
        public virtual ICollection<Product> UpdatedProducts { get; set; } = new List<Product>();

        // Categories created/updated by this user
        public virtual ICollection<Category> CreatedCategories { get; set; } = new List<Category>();
        public virtual ICollection<Category> UpdatedCategories { get; set; } = new List<Category>();

        // Full Name
        public string FullName => $"{FirstName} {LastName}";
    }
}