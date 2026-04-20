namespace PPECB.CatalogManager.Core.DTOs
{
    public class UpdateProductImageDto
    {
        public int Id { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}