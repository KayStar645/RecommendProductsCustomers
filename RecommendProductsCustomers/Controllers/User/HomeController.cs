using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Customer
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> Index([FromQuery] string? pKeyword = "")
        {
            // Sau này sửa thành sản phẩm đề xuất
            ViewData["recommendedProducts"] = await _productService.GetList(pKeyword);
            return View();
        }

        public async Task<IActionResult> Products([FromQuery] string? pKeyword = "")
        {
            ViewData["listProduct"] = await _productService.GetList(pKeyword);
            return View();
        }
    }
}
