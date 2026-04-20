using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Supplier?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Code == code && !s.IsDeleted);
        }

        public async Task<Supplier?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Email == email && !s.IsDeleted);
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(s => (s.Code.Contains(searchTerm) ||
                            s.Name.Contains(searchTerm) ||
                            s.ContactPerson != null && s.ContactPerson.Contains(searchTerm) ||
                            s.Email != null && s.Email.Contains(searchTerm)) && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(s => s.Code == code && !s.IsDeleted &&
                (excludeId == null || s.Id != excludeId));
        }

        public async Task<int> GetProductCountAsync(int supplierId)
        {
            return await _context.Products.CountAsync(p => p.SupplierId == supplierId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Supplier>> GetTopSuppliersByProductCountAsync(int topCount = 10)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    Supplier = s,
                    ProductCount = _context.Products.Count(p => p.SupplierId == s.Id && !p.IsDeleted)
                })
                .OrderByDescending(x => x.ProductCount)
                .Take(topCount)
                .Select(x => x.Supplier)
                .ToListAsync();
        }

        public async Task UpdateSupplierStatusAsync(int supplierId, bool isActive)
        {
            var supplier = await GetByIdAsync(supplierId);
            if (supplier != null)
            {
                supplier.IsActive = isActive;
                Update(supplier);
            }
        }
    }
}