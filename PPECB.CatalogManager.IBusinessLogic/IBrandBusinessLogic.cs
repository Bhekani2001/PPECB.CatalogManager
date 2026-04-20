using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IBrandBusinessLogic
    {
        Task<BrandDto?> GetBrandByIdAsync(int id);
        Task<BrandDto?> GetBrandByNameAsync(string name);
        Task<IEnumerable<BrandDto>> GetAllBrandsAsync();
        Task<IEnumerable<BrandDto>> GetActiveBrandsAsync();
        Task<BrandDto> CreateBrandAsync(CreateBrandDto createDto, string createdBy);
        Task<BrandDto> UpdateBrandAsync(UpdateBrandDto updateDto, string updatedBy);
        Task<bool> DeleteBrandAsync(int id, string deletedBy);
        Task<IEnumerable<BrandDto>> SearchBrandsAsync(string searchTerm);
        Task<bool> IsBrandNameUniqueAsync(string name, int? excludeId = null);
        Task<int> GetBrandProductCountAsync(int brandId);
        Task UpdateBrandStatusAsync(int brandId, bool isActive);
    }
}