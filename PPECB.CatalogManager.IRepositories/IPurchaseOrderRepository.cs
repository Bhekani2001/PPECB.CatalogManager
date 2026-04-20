using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
    {
        Task<PurchaseOrder?> GetByPONumberAsync(string poNumber);
        Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId);
        Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersByStatusAsync(string status);
        Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PagedResultDto<PurchaseOrder>> GetPagedPurchaseOrdersAsync(int pageNumber, int pageSize = 10);
        Task<PurchaseOrder?> GetPurchaseOrderWithItemsAsync(int purchaseOrderId);
        Task<bool> IsPONumberUniqueAsync(string poNumber, int? excludeId = null);
        Task UpdatePurchaseOrderStatusAsync(int purchaseOrderId, string status);
        Task UpdateDeliveryDateAsync(int purchaseOrderId, DateTime? expectedDate, DateTime? actualDate);
        Task<decimal> GetTotalPurchaseOrderValueAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PurchaseOrder>> GetPendingPurchaseOrdersAsync();
    }
}