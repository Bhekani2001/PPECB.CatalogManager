using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class BrandRepository : Repository<Brand>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == name && !b.IsDeleted);
        }

        public async Task<IEnumerable<Brand>> GetActiveBrandsAsync()
        {
            return await _dbSet
                .Where(b => b.IsActive && !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Brand>> SearchBrandsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(b => (b.Name.Contains(searchTerm) ||
                            (b.Description != null && b.Description.Contains(searchTerm))) && !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(b => b.Name == name && !b.IsDeleted &&
                (excludeId == null || b.Id != excludeId));
        }

        public async Task<int> GetProductCountAsync(int brandId)
        {
            return await _context.Products.CountAsync(p => p.BrandId == brandId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Brand>> GetBrandsWithProductCountAsync()
        {
            return await _dbSet
                .Where(b => !b.IsDeleted)
                .Select(b => new Brand
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    LogoUrl = b.LogoUrl,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    Products = _context.Products.Where(p => p.BrandId == b.Id && !p.IsDeleted).ToList()
                })
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task UpdateBrandStatusAsync(int brandId, bool isActive)
        {
            var brand = await GetByIdAsync(brandId);
            if (brand != null)
            {
                brand.IsActive = isActive;
                Update(brand);
            }
        }
    }
}