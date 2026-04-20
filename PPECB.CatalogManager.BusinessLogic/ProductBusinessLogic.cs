using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Data;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;
using PPECB.CatalogManager.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class ProductBusinessLogic : IProductBusinessLogic
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IPriceHistoryRepository _priceHistoryRepository;
        private readonly IMapper _mapper;

        public ProductBusinessLogic(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductImageRepository productImageRepository,
            IPriceHistoryRepository priceHistoryRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
            _priceHistoryRepository = priceHistoryRepository;
            _mapper = mapper;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdWithIncludesAsync(id, p => p.Category);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto?> GetProductByCodeAsync(string code)
        {
            var product = await _productRepository.GetByCodeAsync(code);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto?> GetProductBySlugAsync(string slug)
        {
            var product = await _productRepository.GetBySlugAsync(slug);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithIncludesAsync(p => p.Category);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PagedResultDto<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize = 10)
        {
            var pagedResult = await _productRepository.GetPagedProductsAsync(pageNumber, pageSize);
            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<List<ProductDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<PagedResultDto<ProductDto>> GetPagedProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize = 10)
        {
            var pagedResult = await _productRepository.GetPagedProductsByCategoryAsync(categoryId, pageNumber, pageSize);
            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<List<ProductDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto, string createdBy)
        {
            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {createDto.CategoryId} not found.");
            }

            var product = new Product
            {
                Code = await GenerateProductCodeAsync(),
                Name = createDto.Name,
                Description = createDto.Description,
                SellingPrice = createDto.Price,
                StockQuantity = createDto.StockQuantity,
                CategoryId = createDto.CategoryId,
                Status = ProductStatus.Draft,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                Slug = GenerateSlug(createDto.Name)
            };

            var created = await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto> UpdateProductAsync(UpdateProductDto updateDto, string updatedBy)
        {
            var product = await _productRepository.GetByIdAsync(updateDto.Id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {updateDto.Id} not found.");
            }

            // Validate category exists if changed
            if (product.CategoryId != updateDto.CategoryId)
            {
                var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with ID {updateDto.CategoryId} not found.");
                }
            }

            // Track price change
            if (product.SellingPrice != updateDto.Price)
            {
                await _productRepository.UpdateSellingPriceAsync(updateDto.Id, updateDto.Price, "Price update",
                    int.TryParse(updatedBy, out var userId) ? userId : 0);
            }

            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.SellingPrice = updateDto.Price;
            product.StockQuantity = updateDto.StockQuantity;
            product.Status = updateDto.Status;
            product.CategoryId = updateDto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = updatedBy;
            product.Slug = GenerateSlug(updateDto.Name);

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(int id, string deletedBy)
        {
            return await _productRepository.SoftDeleteAsync(id, deletedBy);
        }

        public async Task<bool> HardDeleteProductAsync(int id)
        {
            return await _productRepository.HardDeleteAsync(id);
        }

        public async Task<string> GenerateProductCodeAsync()
        {
            var yearMonth = DateTime.Now.ToString("yyyyMM");
            var existingProducts = await _productRepository.FindAsync(p => p.Code.StartsWith(yearMonth));
            var maxNumber = existingProducts
                .Select(p => p.Code.Split('-').Last())
                .Where(n => int.TryParse(n, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(0)
                .Max();

            var nextNumber = (maxNumber + 1).ToString("D3");
            return $"{yearMonth}-{nextNumber}";
        }

        public async Task UpdateProductStatusAsync(int id, ProductStatus status, string updatedBy)
        {
            await _productRepository.UpdateProductStatusAsync(id, status);
            await _productRepository.SaveChangesAsync();
        }

        public async Task PublishProductAsync(int id, string publishedBy)
        {
            await _productRepository.UpdatePublishStatusAsync(id, true);
            await _productRepository.SaveChangesAsync();
        }

        public async Task UnpublishProductAsync(int id, string updatedBy)
        {
            await _productRepository.UpdatePublishStatusAsync(id, false);
            await _productRepository.SaveChangesAsync();
        }

        public async Task UpdateStockQuantityAsync(int id, int quantity, string updatedBy)
        {
            await _productRepository.UpdateStockQuantityAsync(id, quantity);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> CheckStockAvailabilityAsync(int id, int requestedQuantity)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null && product.StockQuantity >= requestedQuantity;
        }

        public async Task UpdateProductPriceAsync(int id, decimal newPrice, string reason, string changedBy)
        {
            var userId = int.TryParse(changedBy, out var idUser) ? idUser : 0;
            await _productRepository.UpdateSellingPriceAsync(id, newPrice, reason, userId);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<PagedResultDto<ProductDto>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize = 10)
        {
            var pagedResult = await _productRepository.SearchProductsAsync(searchTerm, pageNumber, pageSize);
            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<List<ProductDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<PagedResultDto<ProductDto>> FilterProductsAsync(ProductFilterDto filter, int pageNumber, int pageSize = 10)
        {
            var pagedResult = await _productRepository.GetFilteredProductsAsync(filter, pageNumber, pageSize);
            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<List<ProductDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<bool> IsProductCodeUniqueAsync(string code, int? excludeId = null)
        {
            return await _productRepository.IsCodeUniqueAsync(code, excludeId);
        }

        public async Task<ProductImageDto?> AddProductImageAsync(int productId, UploadProductImageDto uploadDto)
        {
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

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageData = uploadDto.ImageData,
                ImageMimeType = uploadDto.ContentType,
                FileName = uploadDto.FileName,
                FileSize = uploadDto.ImageData.Length,
                ContentType = uploadDto.ContentType,
                FileExtension = Path.GetExtension(uploadDto.FileName),
                AltText = uploadDto.AltText,
                DisplayOrder = uploadDto.DisplayOrder,
                IsPrimary = uploadDto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            // Get image dimensions
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(uploadDto.ImageData);
                productImage.Width = image.Width;
                productImage.Height = image.Height;
            }
            catch (Exception ex)
            {
                // If we can't read dimensions, just continue
                Console.WriteLine($"Could not read image dimensions: {ex.Message}");
            }

            // If this is the primary image, remove primary flag from others
            if (uploadDto.IsPrimary)
            {
                var existingImages = await _productImageRepository.FindAsync(i => i.ProductId == productId);
                foreach (var img in existingImages)
                {
                    img.IsPrimary = false;
                    _productImageRepository.Update(img);
                }
            }

            var created = await _productImageRepository.AddAsync(productImage);
            await _productImageRepository.SaveChangesAsync();

            return new ProductImageDto
            {
                Id = created.Id,
                ProductId = created.ProductId,
                AltText = created.AltText,
                DisplayOrder = created.DisplayOrder,
                IsPrimary = created.IsPrimary,
                FileSize = created.FileSize,
                ContentType = created.ContentType,
                FileName = created.FileName,
                FileExtension = created.FileExtension,
                Width = created.Width,
                Height = created.Height,
                Base64Image = created.ImageData != null && created.ImageMimeType != null
                    ? $"data:{created.ImageMimeType};base64,{Convert.ToBase64String(created.ImageData)}"
                    : string.Empty
            };
        }
        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            return await _productImageRepository.HardDeleteAsync(imageId);
        }

        public async Task SetPrimaryImageAsync(int productId, int imageId)
        {
            await _productImageRepository.SetPrimaryImageAsync(productId, imageId);
            await _productImageRepository.SaveChangesAsync();
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _productRepository.GetTotalProductCountAsync();
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            return await _productRepository.GetTotalInventoryValueAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10)
        {
            var products = await _productRepository.GetLowStockProductsAsync(threshold);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        private string GenerateSlug(string name)
        {
            return name.ToLower().Replace(" ", "-").Replace("/", "-").Replace("\\", "-");
        }

        public async Task<byte[]> ExportProductsToExcelAsync(ProductFilterDto? filter = null)
        {
            // Get products based on filter
            IEnumerable<ProductDto> products;
            if (filter != null && !string.IsNullOrEmpty(filter.SearchTerm))
            {
                var pagedResult = await SearchProductsAsync(filter.SearchTerm, 1, int.MaxValue);
                products = pagedResult.Items;
            }
            else if (filter != null && filter.CategoryId.HasValue)
            {
                var pagedResult = await GetPagedProductsByCategoryAsync(filter.CategoryId.Value, 1, int.MaxValue);
                products = pagedResult.Items;
            }
            else
            {
                var allProducts = await GetAllProductsAsync();
                products = allProducts;
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            // Headers
            worksheet.Cell(1, 1).Value = "Product Code";
            worksheet.Cell(1, 2).Value = "Product Name";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Category";
            worksheet.Cell(1, 5).Value = "Selling Price";
            worksheet.Cell(1, 6).Value = "Cost Price";
            worksheet.Cell(1, 7).Value = "Stock Quantity";
            worksheet.Cell(1, 8).Value = "Reorder Level";
            worksheet.Cell(1, 9).Value = "Status";
            worksheet.Cell(1, 10).Value = "Published";
            worksheet.Cell(1, 11).Value = "Featured";
            worksheet.Cell(1, 12).Value = "Unit of Measure";
            worksheet.Cell(1, 13).Value = "Created Date";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 13);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            int row = 2;
            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Code;
                worksheet.Cell(row, 2).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Description ?? "";
                worksheet.Cell(row, 4).Value = product.CategoryName;
                worksheet.Cell(row, 5).Value = product.SellingPrice;
                worksheet.Cell(row, 6).Value = product.CostPrice;
                worksheet.Cell(row, 7).Value = product.StockQuantity;
                worksheet.Cell(row, 8).Value = product.ReorderLevel;
                worksheet.Cell(row, 9).Value = product.Status.ToString();
                worksheet.Cell(row, 10).Value = product.IsPublished ? "Yes" : "No";
                worksheet.Cell(row, 11).Value = product.IsFeatured ? "Yes" : "No";
                worksheet.Cell(row, 12).Value = product.UnitOfMeasure ?? "";
                worksheet.Cell(row, 13).Value = product.CreatedAt.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<(int successCount, int failedCount, List<string> errors)> ImportProductsFromExcelAsync(byte[] excelData, string importedBy)
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var categories = await _categoryRepository.GetAllAsync();

            using var stream = new MemoryStream(excelData);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            int rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip headers)
            {
                try
                {
                    var productCode = worksheet.Cell(row, 1)?.GetString()?.Trim();
                    var productName = worksheet.Cell(row, 2)?.GetString()?.Trim();
                    var description = worksheet.Cell(row, 3)?.GetString()?.Trim();
                    var categoryName = worksheet.Cell(row, 4)?.GetString()?.Trim();
                    var sellingPriceStr = worksheet.Cell(row, 5)?.GetString()?.Trim();
                    var costPriceStr = worksheet.Cell(row, 6)?.GetString()?.Trim();
                    var stockQuantityStr = worksheet.Cell(row, 7)?.GetString()?.Trim();
                    var reorderLevelStr = worksheet.Cell(row, 8)?.GetString()?.Trim();
                    var statusStr = worksheet.Cell(row, 9)?.GetString()?.Trim();
                    var publishedStr = worksheet.Cell(row, 10)?.GetString()?.Trim();
                    var featuredStr = worksheet.Cell(row, 11)?.GetString()?.Trim();
                    var unitOfMeasure = worksheet.Cell(row, 12)?.GetString()?.Trim();

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        errors.Add($"Row {row}: Product Name is required.");
                        failedCount++;
                        continue;
                    }

                    // Find category
                    var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                    if (category == null && !string.IsNullOrWhiteSpace(categoryName))
                    {
                        errors.Add($"Row {row}: Category '{categoryName}' not found.");
                        failedCount++;
                        continue;
                    }

                    // Parse price
                    if (!decimal.TryParse(sellingPriceStr, out decimal sellingPrice))
                    {
                        errors.Add($"Row {row}: Invalid Selling Price format.");
                        failedCount++;
                        continue;
                    }

                    // Parse stock quantity
                    if (!int.TryParse(stockQuantityStr, out int stockQuantity))
                    {
                        stockQuantity = 0;
                    }

                    // Parse reorder level
                    if (!int.TryParse(reorderLevelStr, out int reorderLevel))
                    {
                        reorderLevel = 10;
                    }

                    // Parse cost price
                    decimal costPrice = 0;
                    if (!string.IsNullOrWhiteSpace(costPriceStr))
                    {
                        decimal.TryParse(costPriceStr, out costPrice);
                    }

                    // Parse status
                    var status = ProductStatus.Draft;
                    if (!string.IsNullOrWhiteSpace(statusStr))
                    {
                        Enum.TryParse(statusStr, true, out status);
                    }

                    // Parse boolean flags
                    bool isPublished = publishedStr?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true;
                    bool isFeatured = featuredStr?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true;

                    // Check if product already exists by code
                    var existingProduct = await _productRepository.GetByCodeAsync(productCode);

                    if (existingProduct != null)
                    {
                        // Update existing product
                        existingProduct.Name = productName;
                        existingProduct.Description = description;
                        existingProduct.SellingPrice = sellingPrice;
                        existingProduct.CostPrice = costPrice;
                        existingProduct.StockQuantity = stockQuantity;
                        existingProduct.ReorderLevel = reorderLevel;
                        existingProduct.Status = status;
                        existingProduct.IsPublished = isPublished;
                        existingProduct.IsFeatured = isFeatured;
                        existingProduct.UnitOfMeasure = unitOfMeasure;
                        existingProduct.UpdatedAt = DateTime.UtcNow;
                        existingProduct.UpdatedBy = importedBy;

                        if (category != null)
                        {
                            existingProduct.CategoryId = category.Id;
                        }

                        _productRepository.Update(existingProduct);
                        successCount++;
                    }
                    else
                    {
                        // Create new product
                        var newProduct = new Product
                        {
                            Code = await GenerateProductCodeAsync(),
                            Name = productName,
                            Description = description,
                            SellingPrice = sellingPrice,
                            CostPrice = costPrice,
                            StockQuantity = stockQuantity,
                            ReorderLevel = reorderLevel,
                            Status = status,
                            IsPublished = isPublished,
                            IsFeatured = isFeatured,
                            UnitOfMeasure = unitOfMeasure,
                            CategoryId = category.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = importedBy,
                            Slug = GenerateSlug(productName)
                        };

                        await _productRepository.AddAsync(newProduct);
                        successCount++;
                    }

                    await _productRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                    failedCount++;
                }
            }

            return (successCount, failedCount, errors);
        }

}
}