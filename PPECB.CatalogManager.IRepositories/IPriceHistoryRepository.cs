using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IPriceHistoryRepository : IRepository<PriceHistory>
    {
        Task<IEnumerable<PriceHistory>> GetPriceHistoryByProductAsync(int productId);
        Task<IEnumerable<PriceHistory>> GetPriceHistoryByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PriceHistory?> GetLatestPriceChangeAsync(int productId);
        Task<decimal> GetPriceAtDateAsync(int productId, DateTime date);
        Task<IEnumerable<PriceHistory>> GetPriceHistoryWithUserAsync(int productId);
        Task<decimal> GetAveragePriceForProductAsync(int productId, int daysBack = 30);
    }
}