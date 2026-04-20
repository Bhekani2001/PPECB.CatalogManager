using PPECB.CatalogManager.Core.Enums;
using System;
using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }

     
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? TaxRate { get; set; }


        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; } 
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string? DimensionUnit { get; set; }
        public string? MainImageUrl { get; set; }
        public string? ImagePath { get; set; } 

        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public bool IsPublished { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }

   
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }


        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public int? SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

 
        public int? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }


        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }


        public bool IsBatchTracked { get; set; }
        public bool IsSerialTracked { get; set; }

        public bool HasExpiryDate { get; set; }
        public int? ShelfLifeDays { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
    }
}