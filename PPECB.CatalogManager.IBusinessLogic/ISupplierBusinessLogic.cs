using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface ISupplierBusinessLogic
    {
        Task<SupplierDto?> GetSupplierByIdAsync(int id);
        Task<SupplierDto?> GetSupplierByCodeAsync(string code);
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
        Task<IEnumerable<SupplierDto>> GetActiveSuppliersAsync();
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto, string createdBy);
        Task<SupplierDto> UpdateSupplierAsync(UpdateSupplierDto updateDto, string updatedBy);
        Task<bool> DeleteSupplierAsync(int id, string deletedBy);
        Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm);
        Task<bool> IsSupplierCodeUniqueAsync(string code, int? excludeId = null);
        Task<int> GetSupplierProductCountAsync(int supplierId);
        Task UpdateSupplierStatusAsync(int supplierId, bool isActive);
    }
}