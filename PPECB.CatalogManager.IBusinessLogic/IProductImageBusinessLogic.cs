using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IProductImageBusinessLogic
    {
        // Get operations
        Task<ProductImageDto?> GetImageByIdAsync(int id);
        Task<IEnumerable<ProductImageDto>> GetImagesByProductIdAsync(int productId);
        Task<ProductImageDto?> GetPrimaryImageAsync(int productId);
        Task<IEnumerable<ProductImageDto>> GetImagesWithDetailsAsync(int productId);

        // Add/Update/Delete
        Task<ProductImageDto> AddImageAsync(int productId, UploadProductImageDto uploadDto, string uploadedBy);
        Task<ProductImageDto> UpdateImageAsync(int imageId, UpdateProductImageDto updateDto, string updatedBy);
        Task<bool> DeleteImageAsync(int imageId, string deletedBy);
        Task<bool> DeleteAllImagesAsync(int productId, string deletedBy);

        // Image management
        Task SetPrimaryImageAsync(int productId, int imageId);
        Task ReorderImageAsync(int productId, int imageId, int newDisplayOrder);

        // Utilities
        Task<int> GetImageCountAsync(int productId);
        Task<bool> HasImagesAsync(int productId);
        Task<byte[]?> GetImageDataAsync(int imageId, bool useThumbnail = false);
        Task<string> GetImageBase64Async(int imageId, bool useThumbnail = false);

        // Advanced operations
        Task<ProductImageDto> ReplicateImageAsync(int sourceImageId, int targetProductId, string createdBy);
    }
}