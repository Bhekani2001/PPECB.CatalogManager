namespace PPECB.CatalogManager.Core.DTOs
{
    public class UploadProductImageDto
    {
        public int ProductId { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}