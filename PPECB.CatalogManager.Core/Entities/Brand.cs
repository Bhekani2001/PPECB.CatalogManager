using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}