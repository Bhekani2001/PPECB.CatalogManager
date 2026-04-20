using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.Repositories
{
    public class PriceHistoryRepository : Repository<PriceHistory>, IPriceHistoryRepository
    {
        public PriceHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistoryByProductAsync(int productId)
        {
            return await _dbSet
                .Include(ph => ph.Product)
                .Include(ph => ph.ChangedByUser)
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.ChangeDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(ph => ph.Product)
                .Include(ph => ph.ChangedByUser)
                .Where(ph => ph.ChangeDate >= startDate && ph.ChangeDate <= endDate)
                .OrderByDescending(ph => ph.ChangeDate)
                .ToListAsync();
        }

        public async Task<PriceHistory?> GetLatestPriceChangeAsync(int productId)
        {
            return await _dbSet
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.ChangeDate)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetPriceAtDateAsync(int productId, DateTime date)
        {
            var priceHistory = await _dbSet
                .Where(ph => ph.ProductId == productId && ph.ChangeDate <= date)
                .OrderByDescending(ph => ph.ChangeDate)
                .FirstOrDefaultAsync();

            return priceHistory?.NewPrice ?? 0;
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistoryWithUserAsync(int productId)
        {
            return await _dbSet
                .Include(ph => ph.ChangedByUser)
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.ChangeDate)
                .ToListAsync();
        }

        public async Task<decimal> GetAveragePriceForProductAsync(int productId, int daysBack = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
            var prices = await _dbSet
                .Where(ph => ph.ProductId == productId && ph.ChangeDate >= cutoffDate)
                .Select(ph => ph.NewPrice)
                .ToListAsync();

            return prices.Any() ? prices.Average() : 0;
        }
    }
}