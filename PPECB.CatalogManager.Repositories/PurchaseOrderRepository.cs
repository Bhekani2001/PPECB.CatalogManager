using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PurchaseOrder?> GetByPONumberAsync(string poNumber)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(po => po.PONumber == poNumber && !po.IsDeleted);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Where(po => po.SupplierId == supplierId && !po.IsDeleted)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersByStatusAsync(string status)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Where(po => po.Status == status && !po.IsDeleted)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate && !po.IsDeleted)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PagedResultDto<PurchaseOrder>> GetPagedPurchaseOrdersAsync(int pageNumber, int pageSize = 10)
        {
            var query = _dbSet.Include(po => po.Supplier).Where(po => !po.IsDeleted).OrderByDescending(po => po.OrderDate);
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<PurchaseOrder>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderWithItemsAsync(int purchaseOrderId)
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrderId && !po.IsDeleted);
        }

        public async Task<bool> IsPONumberUniqueAsync(string poNumber, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(po => po.PONumber == poNumber && !po.IsDeleted &&
                (excludeId == null || po.Id != excludeId));
        }

        public async Task UpdatePurchaseOrderStatusAsync(int purchaseOrderId, string status)
        {
            var purchaseOrder = await GetByIdAsync(purchaseOrderId);
            if (purchaseOrder != null)
            {
                purchaseOrder.Status = status;
                Update(purchaseOrder);
            }
        }

        public async Task UpdateDeliveryDateAsync(int purchaseOrderId, DateTime? expectedDate, DateTime? actualDate)
        {
            var purchaseOrder = await GetByIdAsync(purchaseOrderId);
            if (purchaseOrder != null)
            {
                if (expectedDate.HasValue)
                    purchaseOrder.ExpectedDeliveryDate = expectedDate;
                if (actualDate.HasValue)
                    purchaseOrder.ActualDeliveryDate = actualDate;
                Update(purchaseOrder);
            }
        }

        public async Task<decimal> GetTotalPurchaseOrderValueAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate && !po.IsDeleted)
                .SumAsync(po => po.TotalAmount);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPendingPurchaseOrdersAsync()
        {
            return await _dbSet
                .Include(po => po.Supplier)
                .Where(po => po.Status == "Pending" && !po.IsDeleted)
                .OrderBy(po => po.OrderDate)
                .ToListAsync();
        }
    }
}