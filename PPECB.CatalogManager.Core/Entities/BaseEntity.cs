using System;
using System.ComponentModel.DataAnnotations;

namespace PPECB.CatalogManager.Core.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public int? TenantId { get; set; }
    }
}