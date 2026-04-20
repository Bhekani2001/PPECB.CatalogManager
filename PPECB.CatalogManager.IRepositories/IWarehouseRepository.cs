using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        Task<Warehouse?> GetByCodeAsync(string code);
        Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
        Task<IEnumerable<Warehouse>> SearchWarehousesAsync(string searchTerm);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<int> GetProductCountAsync(int warehouseId);
        Task UpdateWarehouseStatusAsync(int warehouseId, bool isActive);
        Task<Warehouse?> GetWarehouseWithInventoryAsync(int warehouseId);
    }
}