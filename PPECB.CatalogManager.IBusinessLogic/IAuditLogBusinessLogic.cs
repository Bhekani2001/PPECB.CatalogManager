using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPECB.CatalogManager.Core.DTOs;

namespace PPECB.CatalogManager.IBusinessLogic
{
    public interface IAuditLogBusinessLogic
    {
        // Logging
        Task LogAsync(int? userId, string action, string entityName, string entityId,
                      string? oldValues, string? newValues, string ipAddress, string userAgent);

        Task LogLoginAsync(int userId, string ipAddress, string userAgent, bool success);
        Task LogLogoutAsync(int userId, string ipAddress);
        Task LogEntityChangeAsync<T>(int userId, string action, string entityName,
                                      string entityId, T? oldEntity, T? newEntity, string ipAddress);

        // Retrieval
        Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(int pageNumber, int pageSize = 20);
        Task<IEnumerable<AuditLogDto>> GetUserAuditLogsAsync(int userId);
        Task<IEnumerable<AuditLogDto>> GetEntityAuditLogsAsync(string entityName, int entityId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsByActionAsync(string action);

        // Statistics
        Task<int> GetUserLoginCountAsync(int userId, DateTime startDate, DateTime endDate);
        Task<DateTime?> GetUserLastLoginAsync(int userId);

        // Cleanup
        Task<int> CleanupOldAuditLogsAsync(int daysToKeep = 90);

        // Export
        Task<byte[]> ExportAuditLogsToExcelAsync(DateTime startDate, DateTime endDate);
    }
}