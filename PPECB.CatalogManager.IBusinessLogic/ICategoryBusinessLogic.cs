using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface ICategoryBusinessLogic
    {
        // Basic CRUD operations
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto?> GetCategoryByCodeAsync(string code);
        Task<CategoryDto?> GetCategoryByNameAsync(string name);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();

        // Create, Update, Delete
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto, string createdBy);
        Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateDto, string updatedBy);
        Task<bool> DeleteCategoryAsync(int id, string deletedBy);
        Task<bool> HardDeleteCategoryAsync(int id);

        // Hierarchical operations
        Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentCategoryId);
        Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync();
        Task<string> GetCategoryPathAsync(int categoryId);

        // Validation
        Task<bool> IsCategoryCodeUniqueAsync(string code, int? excludeId = null);
        Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null);
        Task<bool> CanDeleteCategoryAsync(int id);

        // Bulk operations
        Task UpdateCategoryStatusAsync(int id, bool isActive);
        Task UpdateCategoryDisplayOrderAsync(int id, int displayOrder);

        // Search
        Task<IEnumerable<CategoryDto>> SearchCategoriesAsync(string searchTerm);

        // Statistics
        Task<int> GetCategoryProductCountAsync(int categoryId);
        Task<Dictionary<int, int>> GetAllCategoryProductCountsAsync();
    }
}