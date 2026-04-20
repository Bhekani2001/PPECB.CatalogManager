using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class PurchaseOrderItemRepository : Repository<PurchaseOrderItem>, IPurchaseOrderItemRepository
    {
        public PurchaseOrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PurchaseOrderItem>> GetItemsByPurchaseOrderAsync(int purchaseOrderId)
        {
            return await _dbSet
                .Include(item => item.Product)
                .Where(item => item.PurchaseOrderId == purchaseOrderId && !item.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrderItem>> GetItemsByProductAsync(int productId)
        {
            return await _dbSet
                .Include(item => item.PurchaseOrder)
                .Where(item => item.ProductId == productId && !item.IsDeleted)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateReceivedQuantityAsync(int itemId, int receivedQuantity)
        {
            var item = await GetByIdAsync(itemId);
            if (item != null)
            {
                item.ReceivedQuantity = receivedQuantity;
                Update(item);
            }
        }

        public async Task<bool> IsItemFullyReceivedAsync(int purchaseOrderId)
        {
            var items = await GetItemsByPurchaseOrderAsync(purchaseOrderId);
            return items.All(item => item.ReceivedQuantity >= item.Quantity);
        }

        public async Task<int> GetTotalItemsPendingAsync()
        {
            var items = await _dbSet
                .Where(item => item.ReceivedQuantity < item.Quantity && !item.IsDeleted)
                .ToListAsync();

            return items.Sum(item => item.Quantity - (item.ReceivedQuantity ?? 0));
        }
    }
}