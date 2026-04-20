using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductBusinessLogic _productBusinessLogic;
        private readonly ICategoryBusinessLogic _categoryBusinessLogic;

        public ProductsController(IProductBusinessLogic productBusinessLogic, ICategoryBusinessLogic categoryBusinessLogic)
        {
            _productBusinessLogic = productBusinessLogic;
            _categoryBusinessLogic = categoryBusinessLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _productBusinessLogic.GetPagedProductsAsync(pageNumber, pageSize);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productBusinessLogic.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found." });
            return Ok(product);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetProductByCode(string code)
        {
            var product = await _productBusinessLogic.GetProductByCodeAsync(code);
            if (product == null)
                return NotFound(new { message = $"Product with code {code} not found." });
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdBy = "API";
                var product = await _productBusinessLogic.CreateProductAsync(createDto, createdBy);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedBy = "API";
                var product = await _productBusinessLogic.UpdateProductAsync(updateDto, updatedBy);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deletedBy = "API";
            var result = await _productBusinessLogic.DeleteProductAsync(id, deletedBy);

            if (!result)
                return NotFound(new { message = $"Product with ID {id} not found." });

            return Ok(new { message = "Product deleted successfully." });
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _productBusinessLogic.GetPagedProductsByCategoryAsync(categoryId, pageNumber, pageSize);
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term is required." });

            var products = await _productBusinessLogic.SearchProductsAsync(searchTerm, pageNumber, pageSize);
            return Ok(products);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            var products = await _productBusinessLogic.GetLowStockProductsAsync(threshold);
            return Ok(products);
        }

        [HttpPost("publish/{id}")]
        public async Task<IActionResult> PublishProduct(int id)
        {
            try
            {
                var publishedBy = "API";
                await _productBusinessLogic.PublishProductAsync(id, publishedBy);
                return Ok(new { message = "Product published successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
        }

        [HttpPost("unpublish/{id}")]
        public async Task<IActionResult> UnpublishProduct(int id)
        {
            try
            {
                var updatedBy = "API";
                await _productBusinessLogic.UnpublishProductAsync(id, updatedBy);
                return Ok(new { message = "Product unpublished successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] ProductStatus status)
        {
            try
            {
                var updatedBy = "API";
                await _productBusinessLogic.UpdateProductStatusAsync(id, status, updatedBy);
                return Ok(new { message = $"Product status updated to {status}." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string? searchTerm = null, [FromQuery] int? categoryId = null)
        {
            var filter = new ProductFilterDto();
            if (!string.IsNullOrEmpty(searchTerm))
                filter.SearchTerm = searchTerm;
            if (categoryId.HasValue)
                filter.CategoryId = categoryId;

            var excelData = await _productBusinessLogic.ExportProductsToExcelAsync(filter);
            var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}