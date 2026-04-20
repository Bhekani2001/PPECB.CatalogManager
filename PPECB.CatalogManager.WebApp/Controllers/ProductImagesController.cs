using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class ProductImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductBusinessLogic _productBusinessLogic;
        private readonly IProductImageBusinessLogic _productImageBusinessLogic;

        public ProductImagesController(
            ApplicationDbContext context,
            IProductBusinessLogic productBusinessLogic,
            IProductImageBusinessLogic productImageBusinessLogic)
        {
            _context = context;
            _productBusinessLogic = productBusinessLogic;
            _productImageBusinessLogic = productImageBusinessLogic;
        }

        // GET: ProductImages
        public async Task<IActionResult> Index()
        {
            var images = await _context.ProductImages
                .Include(i => i.Product)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return View(images);
        }

        // GET: ProductImages/UploadImage
        public async Task<IActionResult> UploadImage()
        {
            ViewBag.Products = await _productBusinessLogic.GetAllProductsAsync();
            return View();
        }

        // POST: ProductImages/ProcessUploadImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessUploadImage(int productId, IFormFile imageFile, string altText, bool isPrimary)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Please select an image file.";
                return RedirectToAction(nameof(UploadImage));
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP";
                return RedirectToAction(nameof(UploadImage));
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File size cannot exceed 5MB.";
                return RedirectToAction(nameof(UploadImage));
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageData = memoryStream.ToArray();

            var uploadDto = new UploadProductImageDto
            {
                ProductId = productId,
                ImageData = imageData,
                FileName = imageFile.FileName,
                ContentType = imageFile.ContentType,
                AltText = altText,
                DisplayOrder = 0,
                IsPrimary = isPrimary
            };

            var createdBy = User.Identity?.Name ?? "System";
            await _productImageBusinessLogic.AddImageAsync(productId, uploadDto, createdBy);

            TempData["Success"] = "Image uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: ProductImages/DeleteImage/5
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            var result = await _productImageBusinessLogic.DeleteImageAsync(id, deletedBy);

            if (result)
            {
                TempData["Success"] = "Image deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete image.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ProductImages/SetPrimaryImage/5
        [HttpPost]
        public async Task<IActionResult> SetPrimaryImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return NotFound();

            await _productImageBusinessLogic.SetPrimaryImageAsync(image.ProductId, id);
            TempData["Success"] = "Primary image updated!";
            return RedirectToAction(nameof(Index));
        }

        // GET: ProductImages/ViewImage/5
        public async Task<IActionResult> ViewImage(int id)
        {
            var image = await _context.ProductImages
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image == null) return NotFound();

            return View(image);
        }
    }
}