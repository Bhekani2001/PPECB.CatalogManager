using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IBrandRepository : IRepository<Brand>
    {
        Task<Brand?> GetByNameAsync(string name);
        Task<IEnumerable<Brand>> GetActiveBrandsAsync();
        Task<IEnumerable<Brand>> SearchBrandsAsync(string searchTerm);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<int> GetProductCountAsync(int brandId);
        Task<IEnumerable<Brand>> GetBrandsWithProductCountAsync();
        Task UpdateBrandStatusAsync(int brandId, bool isActive);
    }
}