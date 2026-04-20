using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        // Get primary image for a product
        Task<ProductImage?> GetPrimaryImageAsync(int productId);

        // Get all images for a product
        Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(int productId);

        // Get image by product ID and display order
        Task<ProductImage?> GetImageByProductIdAndOrderAsync(int productId, int displayOrder);

        // Set an image as primary (removes primary flag from others)
        Task SetPrimaryImageAsync(int productId, int imageId);

        // Reorder images
        Task ReorderImagesAsync(int productId, int imageId, int newDisplayOrder);

        // Get image count for a product
        Task<int> GetImageCountAsync(int productId);

        // Delete all images for a product
        Task DeleteAllProductImagesAsync(int productId);

        // Get images with full details (including binary data if needed)
        Task<IEnumerable<ProductImage>> GetImagesWithDetailsAsync(int productId);

        // Get primary image with binary data
        Task<ProductImage?> GetPrimaryImageWithDataAsync(int productId);

        // Check if product has any images
        Task<bool> HasImagesAsync(int productId);

        // Get total images count across all products
        Task<int> GetTotalImagesCountAsync();

        // Get images by date range
        Task<IEnumerable<ProductImage>> GetImagesByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Update image alt text
        Task UpdateImageAltTextAsync(int imageId, string altText);

        // Get images by MIME type
        Task<IEnumerable<ProductImage>> GetImagesByMimeTypeAsync(string mimeType);
    }
}