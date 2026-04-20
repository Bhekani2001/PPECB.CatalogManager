using System;

namespace PPECB.CatalogManager.Core.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;


        public byte[]? ImageData { get; set; }  
        public string? ImageMimeType { get; set; } 
        public string? ImageUrl { get; set; }
        public string? ImagePath { get; set; }

 
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }

        public long FileSize { get; set; } 
        public string? ContentType { get; set; }  
        public string? FileName { get; set; }  
        public string? FileExtension { get; set; }  
        public int? Width { get; set; }
        public int? Height { get; set; }

        public byte[]? ThumbnailData { get; set; }
        public string? ThumbnailMimeType { get; set; }

        public bool IsStoredInDatabase => ImageData != null && ImageData.Length > 0;

        public string Base64Image => ImageData != null && !string.IsNullOrEmpty(ImageMimeType)
            ? $"data:{ImageMimeType};base64,{Convert.ToBase64String(ImageData)}"
            : string.Empty;
    }
}