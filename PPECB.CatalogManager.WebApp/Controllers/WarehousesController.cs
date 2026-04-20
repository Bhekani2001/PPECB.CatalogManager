using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly IWarehouseBusinessLogic _warehouseBusinessLogic;

        public WarehousesController(IWarehouseBusinessLogic warehouseBusinessLogic)
        {
            _warehouseBusinessLogic = warehouseBusinessLogic;
        }

        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseBusinessLogic.GetAllWarehousesAsync();
            return View(warehouses);
        }

        public async Task<IActionResult> GetWarehouseDetails(int id)
        {
            var warehouse = await _warehouseBusinessLogic.GetWarehouseByIdAsync(id);
            if (warehouse == null) return NotFound();
            return View(warehouse);
        }

        public IActionResult ShowCreateWarehouseForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateWarehouse(CreateWarehouseDto createDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdBy = User.Identity?.Name ?? "System";
                    await _warehouseBusinessLogic.CreateWarehouseAsync(createDto, createdBy);
                    TempData["Success"] = "Warehouse created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View("ShowCreateWarehouseForm", createDto);
        }

        public async Task<IActionResult> ShowEditWarehouseForm(int id)
        {
            var warehouse = await _warehouseBusinessLogic.GetWarehouseByIdAsync(id);
            if (warehouse == null) return NotFound();

            var updateDto = new UpdateWarehouseDto
            {
                Id = warehouse.Id,
                Code = warehouse.Code,
                Name = warehouse.Name,
                Location = warehouse.Location,
                Address = warehouse.Address,
                ManagerName = warehouse.ManagerName,
                Phone = warehouse.Phone,
                IsActive = warehouse.IsActive
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditWarehouse(int id, UpdateWarehouseDto updateDto)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedBy = User.Identity?.Name ?? "System";
                    await _warehouseBusinessLogic.UpdateWarehouseAsync(updateDto, updatedBy);
                    TempData["Success"] = "Warehouse updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View("ShowEditWarehouseForm", updateDto);
        }

        public async Task<IActionResult> ConfirmDeleteWarehouse(int id)
        {
            var warehouse = await _warehouseBusinessLogic.GetWarehouseByIdAsync(id);
            if (warehouse == null) return NotFound();
            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeleteWarehouse(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _warehouseBusinessLogic.DeleteWarehouseAsync(id, deletedBy);
            TempData["Success"] = "Warehouse deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchWarehouses(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction(nameof(Index));

            var warehouses = await _warehouseBusinessLogic.SearchWarehousesAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", warehouses);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWarehouseStatus(int id)
        {
            var warehouse = await _warehouseBusinessLogic.GetWarehouseByIdAsync(id);
            if (warehouse == null) return NotFound();

            await _warehouseBusinessLogic.UpdateWarehouseStatusAsync(id, !warehouse.IsActive);
            TempData["Success"] = $"Warehouse {(warehouse.IsActive ? "deactivated" : "activated")} successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}