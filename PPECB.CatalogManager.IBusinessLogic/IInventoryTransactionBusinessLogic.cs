using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IInventoryTransactionBusinessLogic
    {
        Task<InventoryTransactionDto?> GetTransactionByIdAsync(int id);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByProductAsync(int productId);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByWarehouseAsync(int warehouseId);
        Task<PagedResultDto<InventoryTransactionDto>> GetPagedTransactionsAsync(int pageNumber, int pageSize = 20);

        Task<InventoryTransactionDto> AddStockAsync(int productId, int quantity, int? warehouseId, string referenceNumber, string notes, int performedByUserId);
        Task<InventoryTransactionDto> RemoveStockAsync(int productId, int quantity, int? warehouseId, string referenceNumber, string notes, int performedByUserId);
        Task<InventoryTransactionDto> TransferStockAsync(int productId, int quantity, int fromWarehouseId, int toWarehouseId, string referenceNumber, string notes, int performedByUserId);
        Task<InventoryTransactionDto> AdjustStockAsync(int productId, int newQuantity, string reason, int performedByUserId);

        Task<int> GetCurrentStockAsync(int productId, int? warehouseId = null);
        Task<decimal> GetTotalStockValueAsync();
        Task<decimal> GetTotalStockValueByWarehouseAsync(int warehouseId);

        Task<IEnumerable<InventoryTransactionDto>> GetProductMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);
    }
}