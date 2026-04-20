using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogBusinessLogic _auditLogBusinessLogic;

        public AuditLogsController(IAuditLogBusinessLogic auditLogBusinessLogic)
        {
            _auditLogBusinessLogic = auditLogBusinessLogic;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
        {
            var logs = await _auditLogBusinessLogic.GetAuditLogsAsync(pageNumber, pageSize);
            return View(logs);
        }

        public async Task<IActionResult> GetUserAuditLogs(int userId)
        {
            var logs = await _auditLogBusinessLogic.GetUserAuditLogsAsync(userId);
            return View("Index", logs);
        }

        public async Task<IActionResult> GetEntityAuditLogs(string entityName, int entityId)
        {
            var logs = await _auditLogBusinessLogic.GetEntityAuditLogsAsync(entityName, entityId);
            return View("Index", logs);
        }

        [HttpPost]
        public async Task<IActionResult> CleanupOldLogs(int daysToKeep = 90)
        {
            var deletedCount = await _auditLogBusinessLogic.CleanupOldAuditLogsAsync(daysToKeep);
            TempData["Success"] = $"Deleted {deletedCount} old audit logs.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportToExcel(DateTime startDate, DateTime endDate)
        {
            var excelData = await _auditLogBusinessLogic.ExportAuditLogsToExcelAsync(startDate, endDate);
            return File(excelData, "text/csv", $"AuditLogs_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
        }
    }
}