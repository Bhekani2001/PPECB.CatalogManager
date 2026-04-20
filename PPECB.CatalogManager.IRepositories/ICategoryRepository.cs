using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // Specific category queries
        Task<Category?> GetByCodeAsync(string code);
        Task<Category?> GetByNameAsync(string name);
        Task<Category?> GetBySlugAsync(string slug);

        // Hierarchical queries
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId);
        Task<IEnumerable<Category>> GetCategoriesByLevelAsync(int level);

        // Active categories
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetFeaturedCategoriesAsync();

        // Search
        Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);

        // Validation
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);

        // Get with products count
        Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync();
        Task<int> GetProductCountAsync(int categoryId);

        // Bulk operations
        Task UpdateDisplayOrderAsync(int categoryId, int displayOrder);
        Task UpdateActiveStatusAsync(int categoryId, bool isActive);

        // Hierarchy path management
        Task<string> GetCategoryPathAsync(int categoryId);
        Task UpdateCategoryPathAsync(int categoryId, string path);
    }
}