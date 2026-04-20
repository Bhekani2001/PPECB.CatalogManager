using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Warehouse?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.Code == code && !w.IsDeleted);
        }

        public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
        {
            return await _dbSet
                .Where(w => w.IsActive && !w.IsDeleted)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse>> SearchWarehousesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(w => (w.Code.Contains(searchTerm) ||
                            w.Name.Contains(searchTerm) ||
                            w.Location != null && w.Location.Contains(searchTerm)) && !w.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(w => w.Code == code && !w.IsDeleted &&
                (excludeId == null || w.Id != excludeId));
        }

        public async Task<int> GetProductCountAsync(int warehouseId)
        {
            return await _context.Products.CountAsync(p => p.WarehouseId == warehouseId && !p.IsDeleted);
        }

        public async Task UpdateWarehouseStatusAsync(int warehouseId, bool isActive)
        {
            var warehouse = await GetByIdAsync(warehouseId);
            if (warehouse != null)
            {
                warehouse.IsActive = isActive;
                Update(warehouse);
            }
        }

        public async Task<Warehouse?> GetWarehouseWithInventoryAsync(int warehouseId)
        {
            return await _dbSet
                .Include(w => w.InventoryTransactions)
                .FirstOrDefaultAsync(w => w.Id == warehouseId && !w.IsDeleted);
        }
    }
}