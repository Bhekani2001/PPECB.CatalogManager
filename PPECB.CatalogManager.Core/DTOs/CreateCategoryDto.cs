namespace PPECB.CatalogManager.Core.DTOs
{
    public class CreateCategoryDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public string? IconUrl { get; set; }
        public string? BannerImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
    }
}