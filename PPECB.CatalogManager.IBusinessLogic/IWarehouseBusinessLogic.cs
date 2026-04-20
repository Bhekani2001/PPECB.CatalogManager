using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IWarehouseBusinessLogic
    {
        Task<WarehouseDto?> GetWarehouseByIdAsync(int id);
        Task<WarehouseDto?> GetWarehouseByCodeAsync(string code);
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
        Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync();
        Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto createDto, string createdBy);
        Task<WarehouseDto> UpdateWarehouseAsync(UpdateWarehouseDto updateDto, string updatedBy);
        Task<bool> DeleteWarehouseAsync(int id, string deletedBy);
        Task<IEnumerable<WarehouseDto>> SearchWarehousesAsync(string searchTerm);
        Task<bool> IsWarehouseCodeUniqueAsync(string code, int? excludeId = null);
        Task<int> GetWarehouseProductCountAsync(int warehouseId);
        Task UpdateWarehouseStatusAsync(int warehouseId, bool isActive);
    }
}