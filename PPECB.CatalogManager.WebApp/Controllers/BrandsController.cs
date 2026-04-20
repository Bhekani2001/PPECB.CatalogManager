using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class BrandsController : Controller
    {
        private readonly IBrandBusinessLogic _brandBusinessLogic;

        public BrandsController(IBrandBusinessLogic brandBusinessLogic)
        {
            _brandBusinessLogic = brandBusinessLogic;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _brandBusinessLogic.GetAllBrandsAsync();
            return View(brands);
        }

        public async Task<IActionResult> GetBrandDetails(int id)
        {
            var brand = await _brandBusinessLogic.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        public IActionResult ShowCreateBrandForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateBrand(CreateBrandDto createDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdBy = User.Identity?.Name ?? "System";
                    await _brandBusinessLogic.CreateBrandAsync(createDto, createdBy);
                    TempData["Success"] = "Brand created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View("ShowCreateBrandForm", createDto);
        }

        public async Task<IActionResult> ShowEditBrandForm(int id)
        {
            var brand = await _brandBusinessLogic.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            var updateDto = new UpdateBrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                IsActive = brand.IsActive
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditBrand(int id, UpdateBrandDto updateDto)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedBy = User.Identity?.Name ?? "System";
                    await _brandBusinessLogic.UpdateBrandAsync(updateDto, updatedBy);
                    TempData["Success"] = "Brand updated successfully!";
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
            return View("ShowEditBrandForm", updateDto);
        }

        public async Task<IActionResult> ConfirmDeleteBrand(int id)
        {
            var brand = await _brandBusinessLogic.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            var productCount = await _brandBusinessLogic.GetBrandProductCountAsync(id);
            if (productCount > 0)
            {
                TempData["Error"] = $"Cannot delete brand with {productCount} associated products.";
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeleteBrand(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _brandBusinessLogic.DeleteBrandAsync(id, deletedBy);
            TempData["Success"] = "Brand deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchBrands(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction(nameof(Index));

            var brands = await _brandBusinessLogic.SearchBrandsAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", brands);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBrandStatus(int id)
        {
            var brand = await _brandBusinessLogic.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            await _brandBusinessLogic.UpdateBrandStatusAsync(id, !brand.IsActive);
            TempData["Success"] = $"Brand {(brand.IsActive ? "deactivated" : "activated")} successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}