using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        // Basic queries
        Task<Product?> GetByCodeAsync(string code);
        Task<Product?> GetBySlugAsync(string slug);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsBySupplierAsync(int supplierId);
        Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId);

        // Pagination (10 per page as required)
        Task<PagedResultDto<Product>> GetPagedProductsAsync(int pageNumber, int pageSize = 10);
        Task<PagedResultDto<Product>> GetPagedProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize = 10);

        // Status queries
        Task<IEnumerable<Product>> GetProductsByStatusAsync(ProductStatus status);
        Task<IEnumerable<Product>> GetPublishedProductsAsync();
        Task<IEnumerable<Product>> GetFeaturedProductsAsync();
        Task<IEnumerable<Product>> GetProductsOnSaleAsync();

        // Inventory queries
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
        Task<IEnumerable<Product>> GetOutOfStockProductsAsync();

        // Price range queries
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        // Search
        Task<PagedResultDto<Product>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize = 10);

        // Validation
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);

        // Bulk operations
        Task UpdateStockQuantityAsync(int productId, int quantity);
        Task UpdateProductStatusAsync(int productId, ProductStatus status);
        Task UpdatePublishStatusAsync(int productId, bool isPublished);

        // Price management
        Task UpdateSellingPriceAsync(int productId, decimal newPrice, string reason, int changedByUserId);

        // Advanced filtering
        Task<PagedResultDto<Product>> GetFilteredProductsAsync(ProductFilterDto filter, int pageNumber, int pageSize = 10);

        // Analytics
        Task<int> GetTotalProductCountAsync();
        Task<decimal> GetAverageProductPriceAsync();
        Task<decimal> GetTotalInventoryValueAsync();

        // Images and variants
        Task<ProductImage?> GetProductImageByIdAsync(int imageId);
        Task<ProductVariant?> GetProductVariantBySKUAsync(string sku);
        Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId);
        Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId);
    }
}