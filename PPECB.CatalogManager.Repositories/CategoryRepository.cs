using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _dbSet.Where(c => c.ParentCategoryId == null && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId)
        {
            return await _dbSet.Where(c => c.ParentCategoryId == parentCategoryId && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByLevelAsync(int level)
        {
            return await _dbSet.Where(c => c.Level == level && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet.Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetFeaturedCategoriesAsync()
        {
            return await _dbSet.Where(c => c.IsFeatured && c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
        {
            return await _dbSet.Where(c => (c.Code.Contains(searchTerm) ||
                                           c.Name.Contains(searchTerm) ||
                                           (c.Description != null && c.Description.Contains(searchTerm))) &&
                                           !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(c => c.Code == code && !c.IsDeleted &&
                (excludeId == null || c.Id != excludeId));
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(c => c.Name == name && !c.IsDeleted &&
                (excludeId == null || c.Id != excludeId));
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync()
        {
            return await _dbSet.Include(c => c.Products)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _context.Products.CountAsync(p => p.CategoryId == categoryId && !p.IsDeleted);
        }

        public async Task UpdateDisplayOrderAsync(int categoryId, int displayOrder)
        {
            var category = await GetByIdAsync(categoryId);
            if (category != null)
            {
                category.DisplayOrder = displayOrder;
                Update(category);
            }
        }

        public async Task UpdateActiveStatusAsync(int categoryId, bool isActive)
        {
            var category = await GetByIdAsync(categoryId);
            if (category != null)
            {
                category.IsActive = isActive;
                Update(category);
            }
        }

        public async Task<string> GetCategoryPathAsync(int categoryId)
        {
            var category = await GetByIdWithIncludesAsync(categoryId, c => c.ParentCategory);
            if (category == null) return string.Empty;

            var path = category.Name;
            var parent = category.ParentCategory;
            while (parent != null)
            {
                path = $"{parent.Name} > {path}";
                parent = parent.ParentCategory;
            }
            return path;
        }

        public async Task UpdateCategoryPathAsync(int categoryId, string path)
        {
            var category = await GetByIdAsync(categoryId);
            if (category != null)
            {
                category.Path = path;
                Update(category);
            }
        }
    }
}