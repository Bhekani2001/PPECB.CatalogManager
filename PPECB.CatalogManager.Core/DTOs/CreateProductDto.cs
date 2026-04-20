namespace PPECB.CatalogManager.Core.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public decimal? TaxRate { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; }
        public int CategoryId { get; set; }
        public int? SupplierId { get; set; }
        public int? BrandId { get; set; }
        public int? WarehouseId { get; set; }
        public bool IsBatchTracked { get; set; }
        public bool IsSerialTracked { get; set; }
        public bool HasExpiryDate { get; set; }
        public int? ShelfLifeDays { get; set; }
    }
}