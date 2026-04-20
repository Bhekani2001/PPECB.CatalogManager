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
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityName, string entityId)
        {
            return await _dbSet
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string action)
        {
            return await _dbSet
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<PagedResultDto<AuditLog>> GetPagedAuditLogsAsync(int pageNumber, int pageSize = 20)
        {
            var query = _dbSet.OrderByDescending(a => a.Timestamp);
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(a => a.User)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<AuditLog>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeForUserAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.UserId == userId && a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetUserLoginHistoryAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId && a.Action == "Login")
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLastLoginAsync(int userId)
        {
            var lastLogin = await _dbSet
                .Where(a => a.UserId == userId && a.Action == "Login")
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync();
            return lastLogin?.Timestamp;
        }

        public async Task<int> GetUserLoginCountAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .CountAsync(a => a.UserId == userId && a.Action == "Login" && a.Timestamp >= startDate && a.Timestamp <= endDate);
        }

        public async Task<IEnumerable<AuditLog>> GetEntityChangeHistoryAsync(string entityName, int entityId)
        {
            return await _dbSet
                .Where(a => a.EntityName == entityName && a.EntityId == entityId.ToString())
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetLatestEntityChangeAsync(string entityName, int entityId)
        {
            return await _dbSet
                .Where(a => a.EntityName == entityName && a.EntityId == entityId.ToString())
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<int> DeleteOldAuditLogsAsync(DateTime olderThan)
        {
            var oldLogs = await _dbSet
                .Where(a => a.Timestamp < olderThan)
                .ToListAsync();
            _dbSet.RemoveRange(oldLogs);
            return oldLogs.Count;
        }

        public async Task<int> GetAuditLogCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .CountAsync(a => a.Timestamp >= startDate && a.Timestamp <= endDate);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsForExportAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .Include(a => a.User)
                .OrderBy(a => a.Timestamp)
                .ToListAsync();
        }
    }
}