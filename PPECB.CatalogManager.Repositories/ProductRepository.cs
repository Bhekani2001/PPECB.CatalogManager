using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using PPECB.CatalogManager.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted);
        }

        public async Task<Product?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Where(p => p.BrandId == brandId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<PagedResultDto<Product>> GetPagedProductsAsync(int pageNumber, int pageSize = 10)
        {
            var query = _dbSet.Where(p => !p.IsDeleted);
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<Product>> GetPagedProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize = 10)
        {
            var query = _dbSet.Where(p => p.CategoryId == categoryId && !p.IsDeleted);
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Product>> GetProductsByStatusAsync(ProductStatus status)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.Status == status && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetPublishedProductsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsPublished && p.Status == ProductStatus.Active && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsFeatured && p.IsPublished && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsOnSaleAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsOnSale && p.DiscountPrice.HasValue && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _dbSet
                .Where(p => p.StockQuantity <= threshold && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetOutOfStockProductsAsync()
        {
            return await _dbSet
                .Where(p => p.StockQuantity == 0 && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.SellingPrice >= minPrice && p.SellingPrice <= maxPrice && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<PagedResultDto<Product>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize = 10)
        {
            var query = _dbSet.Where(p => (p.Code.Contains(searchTerm) ||
                                          p.Name.Contains(searchTerm) ||
                                          (p.Description != null && p.Description.Contains(searchTerm))) &&
                                          !p.IsDeleted);
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(p => p.Code == code && !p.IsDeleted &&
                (excludeId == null || p.Id != excludeId));
        }

        public async Task UpdateStockQuantityAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product != null)
            {
                product.StockQuantity = quantity;
                Update(product);
            }
        }

        public async Task UpdateProductStatusAsync(int productId, ProductStatus status)
        {
            var product = await GetByIdAsync(productId);
            if (product != null)
            {
                product.Status = status;
                if (status == ProductStatus.Active && !product.IsPublished)
                {
                    product.IsPublished = true;
                    product.PublishedDate = DateTime.UtcNow;
                }
                Update(product);
            }
        }

        public async Task UpdatePublishStatusAsync(int productId, bool isPublished)
        {
            var product = await GetByIdAsync(productId);
            if (product != null)
            {
                product.IsPublished = isPublished;
                if (isPublished && product.PublishedDate == null)
                {
                    product.PublishedDate = DateTime.UtcNow;
                }
                Update(product);
            }
        }

        public async Task UpdateSellingPriceAsync(int productId, decimal newPrice, string reason, int changedByUserId)
        {
            var product = await GetByIdAsync(productId);
            if (product != null)
            {
                var oldPrice = product.SellingPrice;
                product.SellingPrice = newPrice;
                Update(product);

                // Add to price history
                var priceHistory = new PriceHistory
                {
                    ProductId = productId,
                    OldPrice = oldPrice,
                    NewPrice = newPrice,
                    ChangeDate = DateTime.UtcNow,
                    Reason = reason,
                    ChangedByUserId = changedByUserId
                };
                await _context.PriceHistories.AddAsync(priceHistory);
            }
        }

        public async Task<PagedResultDto<Product>> GetFilteredProductsAsync(ProductFilterDto filter, int pageNumber, int pageSize = 10)
        {
            var query = _dbSet.Where(p => !p.IsDeleted);

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);
            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId);
            if (filter.SupplierId.HasValue)
                query = query.Where(p => p.SupplierId == filter.SupplierId);
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.SellingPrice >= filter.MinPrice);
            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.SellingPrice <= filter.MaxPrice);
            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(p => p.Name.Contains(filter.SearchTerm) || p.Code.Contains(filter.SearchTerm));

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _dbSet.CountAsync(p => !p.IsDeleted);
        }

        public async Task<decimal> GetAverageProductPriceAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).AverageAsync(p => p.SellingPrice);
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            var products = await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
            return products.Sum(p => p.StockQuantity * p.SellingPrice);
        }

        public async Task<ProductImage?> GetProductImageByIdAsync(int imageId)
        {
            return await _context.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId);
        }

        public async Task<ProductVariant?> GetProductVariantBySKUAsync(string sku)
        {
            return await _context.ProductVariants.FirstOrDefaultAsync(v => v.SKU == sku);
        }

        public async Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId)
        {
            return await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.IsActive)
                .ToListAsync();
        }
    }
}
