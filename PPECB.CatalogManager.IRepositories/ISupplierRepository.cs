using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<Supplier?> GetByCodeAsync(string code);
        Task<Supplier?> GetByEmailAsync(string email);
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
        Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<int> GetProductCountAsync(int supplierId);
        Task<IEnumerable<Supplier>> GetTopSuppliersByProductCountAsync(int topCount = 10);
        Task UpdateSupplierStatusAsync(int supplierId, bool isActive);
    }
}