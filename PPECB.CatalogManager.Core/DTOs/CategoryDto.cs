using System;
using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int Level { get; set; }
        public string? Path { get; set; }
        public string? IconUrl { get; set; }
        public string? BannerImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public bool IsFeatured { get; set; }
        public int? ProductCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}