using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProductImage?> GetPrimaryImageAsync(int productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.IsPrimary && !i.IsDeleted);
        }

        public async Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(int productId)
        {
            return await _dbSet
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetImageByProductIdAndOrderAsync(int productId, int displayOrder)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.DisplayOrder == displayOrder && !i.IsDeleted);
        }

        public async Task SetPrimaryImageAsync(int productId, int imageId)
        {
            // Remove primary flag from all images of this product
            var images = await _dbSet
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .ToListAsync();

            foreach (var image in images)
            {
                image.IsPrimary = false;
                Update(image);
            }

            // Set the selected image as primary
            var primaryImage = await GetByIdAsync(imageId);
            if (primaryImage != null)
            {
                primaryImage.IsPrimary = true;
                Update(primaryImage);
            }

            await SaveChangesAsync();
        }

        public async Task ReorderImagesAsync(int productId, int imageId, int newDisplayOrder)
        {
            var image = await GetByIdAsync(imageId);
            if (image != null)
            {
                image.DisplayOrder = newDisplayOrder;
                Update(image);
                await SaveChangesAsync();
            }
        }

        public async Task<int> GetImageCountAsync(int productId)
        {
            return await _dbSet.CountAsync(i => i.ProductId == productId && !i.IsDeleted);
        }

        public async Task DeleteAllProductImagesAsync(int productId)
        {
            var images = await _dbSet
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .ToListAsync();

            foreach (var image in images)
            {
                HardDelete(image);
            }

            await SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductImage>> GetImagesWithDetailsAsync(int productId)
        {
            return await _dbSet
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ProductImage
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageData = i.ImageData,
                    ImageMimeType = i.ImageMimeType,
                    ImageUrl = i.ImageUrl,
                    ImagePath = i.ImagePath,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary,
                    FileSize = i.FileSize,
                    ContentType = i.ContentType,
                    FileName = i.FileName,
                    FileExtension = i.FileExtension,
                    Width = i.Width,
                    Height = i.Height,
                    ThumbnailData = i.ThumbnailData,
                    ThumbnailMimeType = i.ThumbnailMimeType
                })
                .ToListAsync();
        }

        public async Task<ProductImage?> GetPrimaryImageWithDataAsync(int productId)
        {
            return await _dbSet
                .Where(i => i.ProductId == productId && i.IsPrimary && !i.IsDeleted)
                .Select(i => new ProductImage
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageData = i.ImageData,
                    ImageMimeType = i.ImageMimeType,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasImagesAsync(int productId)
        {
            return await _dbSet.AnyAsync(i => i.ProductId == productId && !i.IsDeleted);
        }

        public async Task<int> GetTotalImagesCountAsync()
        {
            return await _dbSet.CountAsync(i => !i.IsDeleted);
        }

        public async Task<IEnumerable<ProductImage>> GetImagesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateImageAltTextAsync(int imageId, string altText)
        {
            var image = await GetByIdAsync(imageId);
            if (image != null)
            {
                image.AltText = altText;
                Update(image);
                await SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductImage>> GetImagesByMimeTypeAsync(string mimeType)
        {
            return await _dbSet
                .Where(i => i.ImageMimeType == mimeType && !i.IsDeleted)
                .ToListAsync();
        }
    }
}