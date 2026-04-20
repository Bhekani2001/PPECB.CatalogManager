using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class ProductVariant : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string SKU { get; set; } = string.Empty; // Unique variant code
        public string Name { get; set; } = string.Empty;
        public string? Attributes { get; set; } // JSON: {"Color":"Red","Size":"Large"}

        public decimal? AdditionalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}