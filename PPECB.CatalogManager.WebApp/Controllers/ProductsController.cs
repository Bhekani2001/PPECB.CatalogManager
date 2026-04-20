using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.BusinessLogic;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductBusinessLogic _productBusinessLogic;
        private readonly ICategoryBusinessLogic _categoryBusinessLogic;
        private readonly ISupplierBusinessLogic _supplierBusinessLogic;
        private readonly IBrandBusinessLogic _brandBusinessLogic;
        private readonly IWarehouseBusinessLogic _warehouseBusinessLogic;
        private readonly IProductImageBusinessLogic _productImageBusinessLogic;
        private readonly ApplicationDbContext _context;

        public ProductsController(
            IProductBusinessLogic productBusinessLogic,
            ICategoryBusinessLogic categoryBusinessLogic,
            ISupplierBusinessLogic supplierBusinessLogic,
            IBrandBusinessLogic brandBusinessLogic,
            IWarehouseBusinessLogic warehouseBusinessLogic,
            IProductImageBusinessLogic productImageBusinessLogic,
            ApplicationDbContext context)
        {
            _productBusinessLogic = productBusinessLogic;
            _categoryBusinessLogic = categoryBusinessLogic;
            _supplierBusinessLogic = supplierBusinessLogic;
            _brandBusinessLogic = brandBusinessLogic;
            _warehouseBusinessLogic = warehouseBusinessLogic;
            _productImageBusinessLogic = productImageBusinessLogic;
            _context = context;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var products = await _productBusinessLogic.GetPagedProductsAsync(pageNumber, pageSize);
            return View(products);
        }

        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _productBusinessLogic.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var images = await _productImageBusinessLogic.GetImagesByProductIdAsync(id);
            ViewBag.Images = images;
            return View(product);
        }

        public async Task<IActionResult> ShowCreateProductForm()
        {
            ViewBag.Categories = await _categoryBusinessLogic.GetActiveCategoriesAsync();
            ViewBag.Suppliers = await _supplierBusinessLogic.GetAllSuppliersAsync();
            ViewBag.Brands = await _brandBusinessLogic.GetAllBrandsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetAllWarehousesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateProduct(CreateProductDto createDto, List<IFormFile> productImages)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdBy = User.Identity?.Name ?? "System";
                    var product = await _productBusinessLogic.CreateProductAsync(createDto, createdBy);

                    // Upload images if any
                    if (productImages != null && productImages.Any())
                    {
                        for (int i = 0; i < productImages.Count; i++)
                        {
                            var image = productImages[i];
                            var uploadDto = new UploadProductImageDto
                            {
                                ProductId = product.Id,
                                ImageData = await ConvertImageToByteArray(image),
                                FileName = image.FileName,
                                ContentType = image.ContentType,
                                AltText = $"{product.Name} image {i + 1}",
                                DisplayOrder = i + 1,
                                IsPrimary = i == 0 // First image is primary
                            };
                            await _productImageBusinessLogic.AddImageAsync(product.Id, uploadDto, createdBy);
                        }
                    }

                    TempData["Success"] = $"Product '{product.Name}' created successfully! Product Code: {product.Code}";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            ViewBag.Categories = await _categoryBusinessLogic.GetActiveCategoriesAsync();
            ViewBag.Suppliers = await _supplierBusinessLogic.GetAllSuppliersAsync();
            ViewBag.Brands = await _brandBusinessLogic.GetAllBrandsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetAllWarehousesAsync();
            return View("ShowCreateProductForm", createDto);
        }

        public async Task<IActionResult> ShowEditProductForm(int id)
        {
            var product = await _productBusinessLogic.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var updateDto = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                CostPrice = product.CostPrice,
                Price = product.SellingPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                IsPublished = product.IsPublished,
                IsFeatured = product.IsFeatured,
                IsOnSale = product.IsOnSale,
                CategoryId = product.CategoryId
            };

            ViewBag.Categories = await _categoryBusinessLogic.GetActiveCategoriesAsync();
            ViewBag.Suppliers = await _supplierBusinessLogic.GetAllSuppliersAsync();
            ViewBag.Brands = await _brandBusinessLogic.GetAllBrandsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetAllWarehousesAsync();
            ViewBag.ExistingImages = await _productImageBusinessLogic.GetImagesWithDetailsAsync(id);

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditProduct(int id, UpdateProductDto updateDto, List<IFormFile> productImages)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedBy = User.Identity?.Name ?? "System";
                    var product = await _productBusinessLogic.UpdateProductAsync(updateDto, updatedBy);

                    // Upload new images if any
                    if (productImages != null && productImages.Any())
                    {
                        var existingImages = await _productImageBusinessLogic.GetImagesWithDetailsAsync(id);
                        var nextOrder = existingImages.Any() ? existingImages.Max(i => i.DisplayOrder) + 1 : 1;

                        for (int i = 0; i < productImages.Count; i++)
                        {
                            var image = productImages[i];
                            var uploadDto = new UploadProductImageDto
                            {
                                ProductId = product.Id,
                                ImageData = await ConvertImageToByteArray(image),
                                FileName = image.FileName,
                                ContentType = image.ContentType,
                                AltText = $"{product.Name} image {nextOrder + i}",
                                DisplayOrder = nextOrder + i,
                                IsPrimary = !existingImages.Any() && i == 0 // Only primary if no existing images
                            };
                            await _productImageBusinessLogic.AddImageAsync(product.Id, uploadDto, updatedBy);
                        }
                    }

                    TempData["Success"] = $"Product '{product.Name}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            ViewBag.Categories = await _categoryBusinessLogic.GetActiveCategoriesAsync();
            ViewBag.Suppliers = await _supplierBusinessLogic.GetAllSuppliersAsync();
            ViewBag.Brands = await _brandBusinessLogic.GetAllBrandsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetAllWarehousesAsync();
            ViewBag.ExistingImages = await _productImageBusinessLogic.GetImagesWithDetailsAsync(id);
            return View("ShowEditProductForm", updateDto);
        }

        public async Task<IActionResult> ConfirmDeleteProduct(int id)
        {
            var product = await _productBusinessLogic.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeleteProduct(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";

            // Delete all product images first
            await _productImageBusinessLogic.DeleteAllImagesAsync(id, deletedBy);

            // Delete the product
            await _productBusinessLogic.DeleteProductAsync(id, deletedBy);

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchProducts(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction(nameof(Index));

            var products = await _productBusinessLogic.SearchProductsAsync(searchTerm, pageNumber, pageSize);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", products);
        }

        public async Task<IActionResult> FilterByCategory(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            var products = await _productBusinessLogic.GetPagedProductsByCategoryAsync(categoryId, pageNumber, pageSize);
            var category = await _categoryBusinessLogic.GetCategoryByIdAsync(categoryId);
            ViewBag.CategoryName = category?.Name;
            return View("Index", products);
        }

        public async Task<IActionResult> GetLowStockProducts(int threshold = 10)
        {
            var products = await _productBusinessLogic.GetLowStockProductsAsync(threshold);
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> PublishProduct(int id)
        {
            var publishedBy = User.Identity?.Name ?? "System";
            await _productBusinessLogic.PublishProductAsync(id, publishedBy);
            TempData["Success"] = "Product published successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UnpublishProduct(int id)
        {
            var updatedBy = User.Identity?.Name ?? "System";
            await _productBusinessLogic.UnpublishProductAsync(id, updatedBy);
            TempData["Success"] = "Product unpublished successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductStatus(int id, ProductStatus status)
        {
            var updatedBy = User.Identity?.Name ?? "System";
            await _productBusinessLogic.UpdateProductStatusAsync(id, status, updatedBy);
            TempData["Success"] = $"Product status updated to {status}!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/ExportToExcel
        public async Task<IActionResult> ExportToExcel(string? searchTerm = null, int? categoryId = null)
        {
            var filter = new ProductFilterDto();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filter.SearchTerm = searchTerm;
            }

            if (categoryId.HasValue)
            {
                filter.CategoryId = categoryId;
            }

            var excelData = await _productBusinessLogic.ExportProductsToExcelAsync(filter);
            var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: Products/ShowImportExcelForm
        public IActionResult ShowImportExcelForm()
        {
            return View();
        }

        // POST: Products/ImportFromExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file to upload.";
                return RedirectToAction(nameof(ShowImportExcelForm));
            }

            var extension = Path.GetExtension(excelFile.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                TempData["Error"] = "Please upload a valid Excel file (.xlsx or .xls).";
                return RedirectToAction(nameof(ShowImportExcelForm));
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await excelFile.CopyToAsync(memoryStream);
                var excelData = memoryStream.ToArray();

                var importedBy = User.Identity?.Name ?? "System";
                var result = await _productBusinessLogic.ImportProductsFromExcelAsync(excelData, importedBy);

                TempData["Success"] = $"Import completed: {result.successCount} products imported successfully, {result.failedCount} failed.";

                if (result.errors.Any())
                {
                    var errorList = string.Join("; ", result.errors.Take(10));
                    if (result.errors.Count > 10)
                    {
                        errorList += $" and {result.errors.Count - 10} more errors.";
                    }
                    TempData["Warning"] = $"Errors: {errorList}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Products/DeleteProductImage
        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int imageId, int productId)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            var result = await _productImageBusinessLogic.DeleteImageAsync(imageId, deletedBy);

            if (result)
            {
                TempData["Success"] = "Image deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete image.";
            }

            return RedirectToAction(nameof(ShowEditProductForm), new { id = productId });
        }

        // POST: Products/SetPrimaryImage
        [HttpPost]
        public async Task<IActionResult> SetPrimaryImage(int imageId, int productId)
        {
            await _productImageBusinessLogic.SetPrimaryImageAsync(productId, imageId);
            TempData["Success"] = "Primary image updated successfully!";
            return RedirectToAction(nameof(ShowEditProductForm), new { id = productId });
        }

        private async Task<byte[]> ConvertImageToByteArray(IFormFile image)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}