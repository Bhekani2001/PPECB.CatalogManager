using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        // Get audit logs
        Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(int userId);
        Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entityName, string entityId);
        Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string action);
        Task<PagedResultDto<AuditLog>> GetPagedAuditLogsAsync(int pageNumber, int pageSize = 20);

        // Date range queries
        Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeForUserAsync(int userId, DateTime startDate, DateTime endDate);

        // User activity
        Task<IEnumerable<AuditLog>> GetUserLoginHistoryAsync(int userId);
        Task<DateTime?> GetLastLoginAsync(int userId);
        Task<int> GetUserLoginCountAsync(int userId, DateTime startDate, DateTime endDate);

        // Entity change tracking
        Task<IEnumerable<AuditLog>> GetEntityChangeHistoryAsync(string entityName, int entityId);
        Task<AuditLog?> GetLatestEntityChangeAsync(string entityName, int entityId);

        // Cleanup
        Task<int> DeleteOldAuditLogsAsync(DateTime olderThan);
        Task<int> GetAuditLogCountByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Export
        Task<IEnumerable<AuditLog>> GetAuditLogsForExportAsync(DateTime startDate, DateTime endDate);
    }
}