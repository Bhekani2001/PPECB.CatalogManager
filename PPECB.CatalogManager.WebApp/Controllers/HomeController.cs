using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductBusinessLogic _productBusinessLogic;
        private readonly ICategoryBusinessLogic _categoryBusinessLogic;
        private readonly ISupplierBusinessLogic _supplierBusinessLogic;

        public HomeController(
            IProductBusinessLogic productBusinessLogic,
            ICategoryBusinessLogic categoryBusinessLogic,
            ISupplierBusinessLogic supplierBusinessLogic)
        {
            _productBusinessLogic = productBusinessLogic;
            _categoryBusinessLogic = categoryBusinessLogic;
            _supplierBusinessLogic = supplierBusinessLogic;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _productBusinessLogic.GetTotalProductCountAsync();
            ViewBag.TotalCategories = (await _categoryBusinessLogic.GetAllCategoriesAsync()).Count();
            ViewBag.TotalSuppliers = (await _supplierBusinessLogic.GetAllSuppliersAsync()).Count();
            ViewBag.TotalInventoryValue = await _productBusinessLogic.GetTotalInventoryValueAsync();

            var lowStockProducts = await _productBusinessLogic.GetLowStockProductsAsync(10);
            ViewBag.LowStockCount = lowStockProducts.Count();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}