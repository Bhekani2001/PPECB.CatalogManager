namespace PPECB.CatalogManager.Core.Entities
{
    public class ProductAttribute : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string AttributeName { get; set; } = string.Empty; // e.g., "Material"
        public string AttributeValue { get; set; } = string.Empty; // e.g., "Cotton"
        public string? AttributeGroup { get; set; } // e.g., "Technical Specs"
    }
}