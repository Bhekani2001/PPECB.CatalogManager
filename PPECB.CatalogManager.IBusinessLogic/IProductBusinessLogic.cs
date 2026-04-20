using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IProductBusinessLogic
    {
        // Basic CRUD
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto?> GetProductByCodeAsync(string code);
        Task<ProductDto?> GetProductBySlugAsync(string slug);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();

        // Pagination (10 per page as required)
        Task<PagedResultDto<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize = 10);
        Task<PagedResultDto<ProductDto>> GetPagedProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize = 10);

        // Create, Update, Delete
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto, string createdBy);
        Task<ProductDto> UpdateProductAsync(UpdateProductDto updateDto, string updatedBy);
        Task<bool> DeleteProductAsync(int id, string deletedBy);
        Task<bool> HardDeleteProductAsync(int id);

        // Product code generation
        Task<string> GenerateProductCodeAsync();

        // Status operations
        Task UpdateProductStatusAsync(int id, ProductStatus status, string updatedBy);
        Task PublishProductAsync(int id, string publishedBy);
        Task UnpublishProductAsync(int id, string updatedBy);

        // Inventory operations
        Task UpdateStockQuantityAsync(int id, int quantity, string updatedBy);
        Task<bool> CheckStockAvailabilityAsync(int id, int requestedQuantity);

        // Price operations
        Task UpdateProductPriceAsync(int id, decimal newPrice, string reason, string changedBy);

        // Search and Filter
        Task<PagedResultDto<ProductDto>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize = 10);
        Task<PagedResultDto<ProductDto>> FilterProductsAsync(ProductFilterDto filter, int pageNumber, int pageSize = 10);

        // Validation
        Task<bool> IsProductCodeUniqueAsync(string code, int? excludeId = null);

        // Image management
        Task<ProductImageDto?> AddProductImageAsync(int productId, UploadProductImageDto uploadDto);
        Task<bool> DeleteProductImageAsync(int imageId);
        Task SetPrimaryImageAsync(int productId, int imageId);

        // Analytics
        Task<int> GetTotalProductCountAsync();
        Task<decimal> GetTotalInventoryValueAsync();
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);

        // Excel operations
        Task<byte[]> ExportProductsToExcelAsync(ProductFilterDto? filter = null);
        Task<(int successCount, int failedCount, List<string> errors)> ImportProductsFromExcelAsync(byte[] excelData, string importedBy);
    }
}