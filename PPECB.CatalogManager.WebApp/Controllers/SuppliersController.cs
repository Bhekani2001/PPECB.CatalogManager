using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ISupplierBusinessLogic _supplierBusinessLogic;

        public SuppliersController(ISupplierBusinessLogic supplierBusinessLogic)
        {
            _supplierBusinessLogic = supplierBusinessLogic;
        }

        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierBusinessLogic.GetAllSuppliersAsync();
            return View(suppliers);
        }

        public async Task<IActionResult> GetSupplierDetails(int id)
        {
            var supplier = await _supplierBusinessLogic.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        public IActionResult ShowCreateSupplierForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateSupplier(CreateSupplierDto createDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdBy = User.Identity?.Name ?? "System";
                    await _supplierBusinessLogic.CreateSupplierAsync(createDto, createdBy);
                    TempData["Success"] = "Supplier created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View("ShowCreateSupplierForm", createDto);
        }

        public async Task<IActionResult> ShowEditSupplierForm(int id)
        {
            var supplier = await _supplierBusinessLogic.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();

            var updateDto = new UpdateSupplierDto
            {
                Id = supplier.Id,
                Code = supplier.Code,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TaxNumber = supplier.TaxNumber,
                IsActive = supplier.IsActive
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditSupplier(int id, UpdateSupplierDto updateDto)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedBy = User.Identity?.Name ?? "System";
                    await _supplierBusinessLogic.UpdateSupplierAsync(updateDto, updatedBy);
                    TempData["Success"] = "Supplier updated successfully!";
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
            return View("ShowEditSupplierForm", updateDto);
        }

        public async Task<IActionResult> ConfirmDeleteSupplier(int id)
        {
            var supplier = await _supplierBusinessLogic.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();

            var productCount = await _supplierBusinessLogic.GetSupplierProductCountAsync(id);
            if (productCount > 0)
            {
                TempData["Error"] = $"Cannot delete supplier with {productCount} associated products.";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeleteSupplier(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _supplierBusinessLogic.DeleteSupplierAsync(id, deletedBy);
            TempData["Success"] = "Supplier deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchSuppliers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction(nameof(Index));

            var suppliers = await _supplierBusinessLogic.SearchSuppliersAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", suppliers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSupplierStatus(int id)
        {
            var supplier = await _supplierBusinessLogic.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();

            await _supplierBusinessLogic.UpdateSupplierStatusAsync(id, !supplier.IsActive);
            TempData["Success"] = $"Supplier {(supplier.IsActive ? "deactivated" : "activated")} successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}