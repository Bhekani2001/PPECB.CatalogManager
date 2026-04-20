using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class Category : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

 
        public int? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }

        public int Level { get; set; }
        public string? Path { get; set; }


        public string? IconUrl { get; set; }
        public string? BannerImageUrl { get; set; }
        public int DisplayOrder { get; set; }


        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }


        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }

        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}