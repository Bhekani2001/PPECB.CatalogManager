using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
    {
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByProductAsync(int productId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByWarehouseAsync(int warehouseId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByTypeAsync(TransactionType type);
        Task<PagedResultDto<InventoryTransaction>> GetPagedTransactionsAsync(int pageNumber, int pageSize = 20);
        Task<int> GetCurrentStockForProductAsync(int productId, int? warehouseId = null);
        Task<IEnumerable<InventoryTransaction>> GetProductMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalStockValueAsync();
        Task<decimal> GetTotalStockValueByWarehouseAsync(int warehouseId);
    }
}