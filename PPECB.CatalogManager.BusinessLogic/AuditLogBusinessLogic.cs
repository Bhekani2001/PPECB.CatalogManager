using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class AuditLogBusinessLogic : IAuditLogBusinessLogic
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public AuditLogBusinessLogic(IAuditLogRepository auditLogRepository, IMapper mapper)
        {
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
        }

        public async Task LogAsync(int? userId, string action, string entityName, string entityId,
            string? oldValues, string? newValues, string ipAddress, string userAgent)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();
        }

        public async Task LogLoginAsync(int userId, string ipAddress, string userAgent, bool success)
        {
            var action = success ? "Login" : "FailedLogin";
            await LogAsync(userId, action, "User", userId.ToString(), null, null, ipAddress, userAgent);
        }

        public async Task LogLogoutAsync(int userId, string ipAddress)
        {
            await LogAsync(userId, "Logout", "User", userId.ToString(), null, null, ipAddress, null);
        }

        public async Task LogEntityChangeAsync<T>(int userId, string action, string entityName,
            string entityId, T? oldEntity, T? newEntity, string ipAddress)
        {
            var oldJson = oldEntity != null ? JsonConvert.SerializeObject(oldEntity) : null;
            var newJson = newEntity != null ? JsonConvert.SerializeObject(newEntity) : null;

            await LogAsync(userId, action, entityName, entityId, oldJson, newJson, ipAddress, null);
        }

        public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(int pageNumber, int pageSize = 20)
        {
            var pagedResult = await _auditLogRepository.GetPagedAuditLogsAsync(pageNumber, pageSize);
            return new PagedResultDto<AuditLogDto>
            {
                Items = _mapper.Map<List<AuditLogDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<IEnumerable<AuditLogDto>> GetUserAuditLogsAsync(int userId)
        {
            var logs = await _auditLogRepository.GetAuditLogsByUserAsync(userId);
            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        public async Task<IEnumerable<AuditLogDto>> GetEntityAuditLogsAsync(string entityName, int entityId)
        {
            var logs = await _auditLogRepository.GetEntityChangeHistoryAsync(entityName, entityId);
            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _auditLogRepository.GetAuditLogsByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByActionAsync(string action)
        {
            var logs = await _auditLogRepository.GetAuditLogsByActionAsync(action);
            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        public async Task<int> GetUserLoginCountAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _auditLogRepository.GetUserLoginCountAsync(userId, startDate, endDate);
        }

        public async Task<DateTime?> GetUserLastLoginAsync(int userId)
        {
            return await _auditLogRepository.GetLastLoginAsync(userId);
        }

        public async Task<int> CleanupOldAuditLogsAsync(int daysToKeep = 90)
        {
            var olderThan = DateTime.UtcNow.AddDays(-daysToKeep);
            return await _auditLogRepository.DeleteOldAuditLogsAsync(olderThan);
        }

        public async Task<byte[]> ExportAuditLogsToExcelAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _auditLogRepository.GetAuditLogsForExportAsync(startDate, endDate);
            var logDtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);

            // Simple CSV export for now (can be enhanced with EPPlus)
            var csv = "Timestamp,User,Action,EntityName,EntityId,IpAddress\n";
            foreach (var log in logDtos)
            {
                csv += $"{log.Timestamp},{log.UserEmail},{log.Action},{log.EntityName},{log.EntityId},{log.IpAddress}\n";
            }

            return System.Text.Encoding.UTF8.GetBytes(csv);
        }
    }
}