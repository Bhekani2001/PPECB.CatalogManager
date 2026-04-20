using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using System;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
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
        public ProductStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public bool IsBatchTracked { get; set; }
        public bool IsSerialTracked { get; set; }
        public bool HasExpiryDate { get; set; }
        public int? ShelfLifeDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}