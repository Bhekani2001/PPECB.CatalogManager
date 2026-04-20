using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryBusinessLogic _categoryBusinessLogic;

        public CategoriesController(ICategoryBusinessLogic categoryBusinessLogic)
        {
            _categoryBusinessLogic = categoryBusinessLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryBusinessLogic.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryBusinessLogic.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found." });
            return Ok(category);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetCategoryByCode(string code)
        {
            var category = await _categoryBusinessLogic.GetCategoryByCodeAsync(code);
            if (category == null)
                return NotFound(new { message = $"Category with code {code} not found." });
            return Ok(category);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _categoryBusinessLogic.GetActiveCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("roots")]
        public async Task<IActionResult> GetRootCategories()
        {
            var categories = await _categoryBusinessLogic.GetRootCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}/subcategories")]
        public async Task<IActionResult> GetSubCategories(int id)
        {
            var categories = await _categoryBusinessLogic.GetSubCategoriesAsync(id);
            return Ok(categories);
        }

        [HttpGet("hierarchy")]
        public async Task<IActionResult> GetCategoryHierarchy()
        {
            var hierarchy = await _categoryBusinessLogic.GetCategoryHierarchyAsync();
            return Ok(hierarchy);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdBy = "API";
                var category = await _categoryBusinessLogic.CreateCategoryAsync(createDto, createdBy);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedBy = "API";
                var category = await _categoryBusinessLogic.UpdateCategoryAsync(updateDto, updatedBy);
                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var deletedBy = "API";

            var canDelete = await _categoryBusinessLogic.CanDeleteCategoryAsync(id);
            if (!canDelete)
                return BadRequest(new { message = "Cannot delete category with associated products." });

            var result = await _categoryBusinessLogic.DeleteCategoryAsync(id, deletedBy);

            if (!result)
                return NotFound(new { message = $"Category with ID {id} not found." });

            return Ok(new { message = "Category deleted successfully." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCategories([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term is required." });

            var categories = await _categoryBusinessLogic.SearchCategoriesAsync(searchTerm);
            return Ok(categories);
        }
    }
}