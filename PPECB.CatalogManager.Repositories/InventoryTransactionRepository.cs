using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using PPECB.CatalogManager.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.Repositories
{
    public class InventoryTransactionRepository : Repository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByProductAsync(int productId)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Where(t => t.ProductId == productId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByWarehouseAsync(int warehouseId)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Where(t => t.WarehouseId == warehouseId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByTypeAsync(TransactionType type)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<PagedResultDto<InventoryTransaction>> GetPagedTransactionsAsync(int pageNumber, int pageSize = 20)
        {
            var query = _dbSet.Include(t => t.Product).Include(t => t.Warehouse).OrderByDescending(t => t.TransactionDate);
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<InventoryTransaction>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetCurrentStockForProductAsync(int productId, int? warehouseId = null)
        {
            var query = _dbSet.Where(t => t.ProductId == productId);
            if (warehouseId.HasValue)
                query = query.Where(t => t.WarehouseId == warehouseId);

            var transactions = await query.ToListAsync();

            int totalIn = transactions.Where(t => t.Type == TransactionType.Receipt).Sum(t => t.Quantity);
            int totalOut = transactions.Where(t => t.Type == TransactionType.Issue).Sum(t => t.Quantity);

            return totalIn - totalOut;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetProductMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(t => t.Warehouse)
                .Include(t => t.PerformedByUser)
                .Where(t => t.ProductId == productId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            var products = await _context.Products.Where(p => !p.IsDeleted).ToListAsync();
            decimal totalValue = 0;

            foreach (var product in products)
            {
                var stock = await GetCurrentStockForProductAsync(product.Id);
                totalValue += stock * product.CostPrice;
            }

            return totalValue;
        }

        public async Task<decimal> GetTotalStockValueByWarehouseAsync(int warehouseId)
        {
            var products = await _context.Products.Where(p => !p.IsDeleted).ToListAsync();
            decimal totalValue = 0;

            foreach (var product in products)
            {
                var stock = await GetCurrentStockForProductAsync(product.Id, warehouseId);
                totalValue += stock * product.CostPrice;
            }

            return totalValue;
        }
    }
}