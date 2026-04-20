using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IPurchaseOrderItemRepository : IRepository<PurchaseOrderItem>
    {
        Task<IEnumerable<PurchaseOrderItem>> GetItemsByPurchaseOrderAsync(int purchaseOrderId);
        Task<IEnumerable<PurchaseOrderItem>> GetItemsByProductAsync(int productId);
        Task UpdateReceivedQuantityAsync(int itemId, int receivedQuantity);
        Task<bool> IsItemFullyReceivedAsync(int purchaseOrderId);
        Task<int> GetTotalItemsPendingAsync();
    }
}