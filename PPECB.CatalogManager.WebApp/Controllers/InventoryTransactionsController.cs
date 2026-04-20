using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;
using System.Security.Claims;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class InventoryTransactionsController : Controller
    {
        private readonly IInventoryTransactionBusinessLogic _inventoryTransactionBusinessLogic;
        private readonly IProductBusinessLogic _productBusinessLogic;
        private readonly IWarehouseBusinessLogic _warehouseBusinessLogic;
        private readonly IUserBusinessLogic _userBusinessLogic;

        public InventoryTransactionsController(
            IInventoryTransactionBusinessLogic inventoryTransactionBusinessLogic,
            IProductBusinessLogic productBusinessLogic,
            IWarehouseBusinessLogic warehouseBusinessLogic,
            IUserBusinessLogic userBusinessLogic)
        {
            _inventoryTransactionBusinessLogic = inventoryTransactionBusinessLogic;
            _productBusinessLogic = productBusinessLogic;
            _warehouseBusinessLogic = warehouseBusinessLogic;
            _userBusinessLogic = userBusinessLogic;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
        {
            var transactions = await _inventoryTransactionBusinessLogic.GetPagedTransactionsAsync(pageNumber, pageSize);
            return View(transactions);
        }

        public async Task<IActionResult> GetTransactionDetails(int id)
        {
            var transaction = await _inventoryTransactionBusinessLogic.GetTransactionByIdAsync(id);
            if (transaction == null) return NotFound();
            return View(transaction);
        }

        public async Task<IActionResult> ShowAddStockForm()
        {
            ViewBag.Products = await _productBusinessLogic.GetAllProductsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetActiveWarehousesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAddStock(int productId, int quantity, int? warehouseId, string referenceNumber, string notes)
        {
            // Get the current logged-in user
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userBusinessLogic.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var performedByUserId = user.Id;

            await _inventoryTransactionBusinessLogic.AddStockAsync(productId, quantity, warehouseId, referenceNumber, notes, performedByUserId);
            TempData["Success"] = $"Added {quantity} units to stock!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ShowRemoveStockForm()
        {
            ViewBag.Products = await _productBusinessLogic.GetAllProductsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetActiveWarehousesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessRemoveStock(int productId, int quantity, int? warehouseId, string referenceNumber, string notes)
        {
            // Get the current logged-in user
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userBusinessLogic.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var performedByUserId = user.Id;

            try
            {
                await _inventoryTransactionBusinessLogic.RemoveStockAsync(productId, quantity, warehouseId, referenceNumber, notes, performedByUserId);
                TempData["Success"] = $"Removed {quantity} units from stock!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ShowTransferStockForm()
        {
            ViewBag.Products = await _productBusinessLogic.GetAllProductsAsync();
            ViewBag.Warehouses = await _warehouseBusinessLogic.GetActiveWarehousesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransferStock(int productId, int quantity, int fromWarehouseId, int toWarehouseId, string referenceNumber, string notes)
        {
            // Get the current logged-in user
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userBusinessLogic.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var performedByUserId = user.Id;

            await _inventoryTransactionBusinessLogic.TransferStockAsync(productId, quantity, fromWarehouseId, toWarehouseId, referenceNumber, notes, performedByUserId);
            TempData["Success"] = $"Transferred {quantity} units from warehouse {fromWarehouseId} to {toWarehouseId}!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ShowAdjustStockForm()
        {
            ViewBag.Products = await _productBusinessLogic.GetAllProductsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAdjustStock(int productId, int newQuantity, string reason)
        {
            // Get the current logged-in user
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userBusinessLogic.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var performedByUserId = user.Id;

            await _inventoryTransactionBusinessLogic.AdjustStockAsync(productId, newQuantity, reason, performedByUserId);
            TempData["Success"] = $"Stock adjusted to {newQuantity} units!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetCurrentStock(int productId)
        {
            var stock = await _inventoryTransactionBusinessLogic.GetCurrentStockAsync(productId);
            return Json(new { productId, stock });
        }

        public async Task<IActionResult> GetProductTransactions(int productId)
        {
            var transactions = await _inventoryTransactionBusinessLogic.GetTransactionsByProductAsync(productId);
            return View("Index", transactions);
        }

        public async Task<IActionResult> GetWarehouseTransactions(int warehouseId)
        {
            var transactions = await _inventoryTransactionBusinessLogic.GetTransactionsByWarehouseAsync(warehouseId);
            return View("Index", transactions);
        }
    }
}