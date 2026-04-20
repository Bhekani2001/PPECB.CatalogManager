using System;

namespace PPECB.CatalogManager.Core.Entities
{
    public class AuditLog : BaseEntity
    {
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, Logout
        public string EntityName { get; set; } = string.Empty; // Product, Category, User
        public string? EntityId { get; set; }
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}