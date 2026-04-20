using System;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
        public int? ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}