using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryBusinessLogic _categoryBusinessLogic;

        public CategoriesController(ICategoryBusinessLogic categoryBusinessLogic)
        {
            _categoryBusinessLogic = categoryBusinessLogic;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryBusinessLogic.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> GetCategoryDetails(int id)
        {
            var category = await _categoryBusinessLogic.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        public IActionResult ShowCreateCategoryForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateCategory(CreateCategoryDto createDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdBy = User.Identity?.Name ?? "System";
                    await _categoryBusinessLogic.CreateCategoryAsync(createDto, createdBy);
                    TempData["Success"] = "Category created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View("ShowCreateCategoryForm", createDto);
        }

        public async Task<IActionResult> ShowEditCategoryForm(int id)
        {
            var category = await _categoryBusinessLogic.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var updateDto = new UpdateCategoryDto
            {
                Id = category.Id,
                Code = category.Code,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                IsFeatured = category.IsFeatured
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditCategory(int id, UpdateCategoryDto updateDto)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedBy = User.Identity?.Name ?? "System";
                    await _categoryBusinessLogic.UpdateCategoryAsync(updateDto, updatedBy);
                    TempData["Success"] = "Category updated successfully!";
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
            return View("ShowEditCategoryForm", updateDto);
        }

        public async Task<IActionResult> ConfirmDeleteCategory(int id)
        {
            var category = await _categoryBusinessLogic.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var canDelete = await _categoryBusinessLogic.CanDeleteCategoryAsync(id);
            if (!canDelete)
            {
                TempData["Error"] = "Cannot delete category with associated products.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeleteCategory(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _categoryBusinessLogic.DeleteCategoryAsync(id, deletedBy);
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewCategoryHierarchy()
        {
            var hierarchy = await _categoryBusinessLogic.GetCategoryHierarchyAsync();
            return View(hierarchy);
        }

        public async Task<IActionResult> GetSubcategories(int parentId)
        {
            var subCategories = await _categoryBusinessLogic.GetSubCategoriesAsync(parentId);
            return Json(subCategories);
        }

        public async Task<IActionResult> SearchCategories(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction(nameof(Index));

            var categories = await _categoryBusinessLogic.SearchCategoriesAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", categories);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            var category = await _categoryBusinessLogic.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            await _categoryBusinessLogic.UpdateCategoryStatusAsync(id, !category.IsActive);
            TempData["Success"] = $"Category {(category.IsActive ? "deactivated" : "activated")} successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}