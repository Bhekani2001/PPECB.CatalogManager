using System;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
        public string? FileExtension { get; set; }
        public string? Base64Image { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}