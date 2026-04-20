using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class ProductImageBusinessLogic : IProductImageBusinessLogic
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductImageBusinessLogic(
            IProductImageRepository productImageRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductImageDto?> GetImageByIdAsync(int id)
        {
            var image = await _productImageRepository.GetByIdAsync(id);
            return image != null ? MapToDtoWithBase64(image) : null;
        }

        public async Task<IEnumerable<ProductImageDto>> GetImagesByProductIdAsync(int productId)
        {
            var images = await _productImageRepository.GetImagesByProductIdAsync(productId);
            var imageDtos = new List<ProductImageDto>();

            foreach (var image in images)
            {
                imageDtos.Add(MapToDtoWithBase64(image));
            }

            return imageDtos;
        }

        public async Task<ProductImageDto?> GetPrimaryImageAsync(int productId)
        {
            var image = await _productImageRepository.GetPrimaryImageWithDataAsync(productId);
            return image != null ? MapToDtoWithBase64(image) : null;
        }

        public async Task<ProductImageDto> AddImageAsync(int productId, UploadProductImageDto uploadDto, string uploadedBy)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Validate image data
            if (uploadDto.ImageData == null || uploadDto.ImageData.Length == 0)
            {
                throw new ArgumentException("Image data is required.");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(uploadDto.ContentType.ToLower()))
            {
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WEBP are allowed.");
            }

            // Validate file size (max 5MB)
            if (uploadDto.ImageData.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("File size cannot exceed 5MB.");
            }

            // Generate thumbnail
            byte[]? thumbnailData = await GenerateThumbnailAsync(uploadDto.ImageData, uploadDto.ContentType);

            // Check if this is the first image or should be primary
            var imageCount = await _productImageRepository.GetImageCountAsync(productId);
            var isPrimary = uploadDto.IsPrimary || imageCount == 0;

            // Get next display order if not specified
            var displayOrder = uploadDto.DisplayOrder;
            if (displayOrder == 0)
            {
                var existingImages = await _productImageRepository.GetImagesByProductIdAsync(productId);
                displayOrder = existingImages.Any() ? existingImages.Max(i => i.DisplayOrder) + 1 : 1;
            }

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageData = uploadDto.ImageData,
                ThumbnailData = thumbnailData,
                ImageMimeType = uploadDto.ContentType,
                ThumbnailMimeType = uploadDto.ContentType,
                FileName = uploadDto.FileName,
                FileSize = uploadDto.ImageData.Length,
                ContentType = uploadDto.ContentType,
                FileExtension = Path.GetExtension(uploadDto.FileName),
                AltText = uploadDto.AltText,
                DisplayOrder = displayOrder,
                IsPrimary = isPrimary,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = uploadedBy
            };

            // Get image dimensions
            try
            {
                using var image = Image.Load(uploadDto.ImageData);
                productImage.Width = image.Width;
                productImage.Height = image.Height;
            }
            catch (Exception ex)
            {
                // If we can't read dimensions, just continue
                Console.WriteLine($"Could not read image dimensions: {ex.Message}");
            }

            // If this is the primary image, remove primary flag from others
            if (isPrimary)
            {
                var existingImages = await _productImageRepository.GetImagesByProductIdAsync(productId);
                foreach (var img in existingImages)
                {
                    img.IsPrimary = false;
                    _productImageRepository.Update(img);
                }
            }

            var created = await _productImageRepository.AddAsync(productImage);
            await _productImageRepository.SaveChangesAsync();

            return MapToDtoWithBase64(created);
        }

        public async Task<ProductImageDto> UpdateImageAsync(int imageId, UpdateProductImageDto updateDto, string updatedBy)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null)
            {
                throw new KeyNotFoundException($"Image with ID {imageId} not found.");
            }

            // If setting as primary, remove primary flag from other images of the same product
            if (updateDto.IsPrimary && !image.IsPrimary)
            {
                var existingImages = await _productImageRepository.GetImagesByProductIdAsync(image.ProductId);
                foreach (var img in existingImages)
                {
                    img.IsPrimary = false;
                    _productImageRepository.Update(img);
                }
            }

            image.AltText = updateDto.AltText;
            image.DisplayOrder = updateDto.DisplayOrder;
            image.IsPrimary = updateDto.IsPrimary;
            image.UpdatedAt = DateTime.UtcNow;
            image.UpdatedBy = updatedBy;

            _productImageRepository.Update(image);
            await _productImageRepository.SaveChangesAsync();

            var updated = await _productImageRepository.GetByIdAsync(imageId);
            return MapToDtoWithBase64(updated!);
        }

        public async Task<bool> DeleteImageAsync(int imageId, string deletedBy)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null)
            {
                return false;
            }

            var productId = image.ProductId;
            var wasPrimary = image.IsPrimary;

            var result = await _productImageRepository.SoftDeleteAsync(imageId, deletedBy);

            // If the deleted image was primary, set another image as primary
            if (wasPrimary && result)
            {
                var remainingImages = await _productImageRepository.GetImagesByProductIdAsync(productId);
                var firstImage = remainingImages.FirstOrDefault();
                if (firstImage != null)
                {
                    firstImage.IsPrimary = true;
                    _productImageRepository.Update(firstImage);
                    await _productImageRepository.SaveChangesAsync();
                }
            }

            await _productImageRepository.SaveChangesAsync();
            return result;
        }

        public async Task<bool> DeleteAllImagesAsync(int productId, string deletedBy)
        {
            await _productImageRepository.DeleteAllProductImagesAsync(productId);
            await _productImageRepository.SaveChangesAsync();
            return true;
        }

        public async Task SetPrimaryImageAsync(int productId, int imageId)
        {
            await _productImageRepository.SetPrimaryImageAsync(productId, imageId);
            await _productImageRepository.SaveChangesAsync();
        }

        public async Task ReorderImageAsync(int productId, int imageId, int newDisplayOrder)
        {
            await _productImageRepository.ReorderImagesAsync(productId, imageId, newDisplayOrder);
            await _productImageRepository.SaveChangesAsync();
        }

        public async Task<int> GetImageCountAsync(int productId)
        {
            return await _productImageRepository.GetImageCountAsync(productId);
        }

        public async Task<bool> HasImagesAsync(int productId)
        {
            return await _productImageRepository.HasImagesAsync(productId);
        }

        public async Task<byte[]?> GetImageDataAsync(int imageId, bool useThumbnail = false)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null)
            {
                return null;
            }

            return useThumbnail && image.ThumbnailData != null ? image.ThumbnailData : image.ImageData;
        }

        public async Task<string> GetImageBase64Async(int imageId, bool useThumbnail = false)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null)
            {
                return string.Empty;
            }

            var data = useThumbnail && image.ThumbnailData != null ? image.ThumbnailData : image.ImageData;
            var mimeType = useThumbnail && image.ThumbnailData != null ? image.ThumbnailMimeType : image.ImageMimeType;

            if (data == null || string.IsNullOrEmpty(mimeType))
            {
                return string.Empty;
            }

            return $"data:{mimeType};base64,{Convert.ToBase64String(data)}";
        }

        public async Task<IEnumerable<ProductImageDto>> GetImagesWithDetailsAsync(int productId)
        {
            var images = await _productImageRepository.GetImagesWithDetailsAsync(productId);
            var imageDtos = new List<ProductImageDto>();

            foreach (var image in images)
            {
                imageDtos.Add(MapToDtoWithoutData(image));
            }

            return imageDtos;
        }

        public async Task<ProductImageDto> ReplicateImageAsync(int sourceImageId, int targetProductId, string createdBy)
        {
            var sourceImage = await _productImageRepository.GetByIdAsync(sourceImageId);
            if (sourceImage == null)
            {
                throw new KeyNotFoundException($"Source image with ID {sourceImageId} not found.");
            }

            var targetProduct = await _productRepository.GetByIdAsync(targetProductId);
            if (targetProduct == null)
            {
                throw new KeyNotFoundException($"Target product with ID {targetProductId} not found.");
            }

            var newImage = new ProductImage
            {
                ProductId = targetProductId,
                ImageData = sourceImage.ImageData,
                ThumbnailData = sourceImage.ThumbnailData,
                ImageMimeType = sourceImage.ImageMimeType,
                ThumbnailMimeType = sourceImage.ThumbnailMimeType,
                FileName = sourceImage.FileName,
                FileSize = sourceImage.FileSize,
                ContentType = sourceImage.ContentType,
                FileExtension = sourceImage.FileExtension,
                AltText = sourceImage.AltText,
                Width = sourceImage.Width,
                Height = sourceImage.Height,
                DisplayOrder = 0,
                IsPrimary = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            var created = await _productImageRepository.AddAsync(newImage);
            await _productImageRepository.SaveChangesAsync();

            return MapToDtoWithBase64(created);
        }

        #region Private Methods

        private async Task<byte[]?> GenerateThumbnailAsync(byte[] imageData, string mimeType)
        {
            try
            {
                using var stream = new MemoryStream(imageData);
                using var image = await Image.LoadAsync(stream);

                // Calculate thumbnail dimensions (max 200px)
                int width = image.Width;
                int height = image.Height;

                if (width > 200 || height > 200)
                {
                    if (width > height)
                    {
                        height = (int)((double)height / width * 200);
                        width = 200;
                    }
                    else
                    {
                        width = (int)((double)width / height * 200);
                        height = 200;
                    }
                }

                image.Mutate(x => x.Resize(width, height));

                using var outputStream = new MemoryStream();
                await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not generate thumbnail: {ex.Message}");
                return null;
            }
        }

        private ProductImageDto MapToDtoWithBase64(ProductImage image)
        {
            var dto = new ProductImageDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                AltText = image.AltText,
                DisplayOrder = image.DisplayOrder,
                IsPrimary = image.IsPrimary,
                FileSize = image.FileSize,
                ContentType = image.ContentType,
                FileName = image.FileName,
                FileExtension = image.FileExtension,
                Width = image.Width,
                Height = image.Height,
                CreatedAt = image.CreatedAt,
                CreatedBy = image.CreatedBy,
                UpdatedAt = image.UpdatedAt,
                UpdatedBy = image.UpdatedBy
            };

            if (image.ImageData != null && image.ImageMimeType != null)
            {
                dto.Base64Image = $"data:{image.ImageMimeType};base64,{Convert.ToBase64String(image.ImageData)}";
            }

            return dto;
        }

        private ProductImageDto MapToDtoWithoutData(ProductImage image)
        {
            return new ProductImageDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                AltText = image.AltText,
                DisplayOrder = image.DisplayOrder,
                IsPrimary = image.IsPrimary,
                FileSize = image.FileSize,
                ContentType = image.ContentType,
                FileName = image.FileName,
                FileExtension = image.FileExtension,
                Width = image.Width,
                Height = image.Height,
                CreatedAt = image.CreatedAt,
                CreatedBy = image.CreatedBy,
                UpdatedAt = image.UpdatedAt,
                UpdatedBy = image.UpdatedBy
            };
        }

        #endregion
    }
}