using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Services;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Customer
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IAuthService _authService;

        public HomeController(IProductService productService, IAuthService authService)
        {
            _productService = productService;
            _authService = authService;
        }
        public async Task<IActionResult> Index([FromQuery] string? pKeyword = "")
        {
            if (Request.Cookies.TryGetValue("userName", out string userName))
            {
                bool flag = await _authService.Login(new UserModel() { userName = userName });

                if (flag)
                {
                    Response.Cookies.Append("userName", userName, new CookieOptions
                    {
                        Expires = DateTime.Now.AddYears(10)
                    });

                    string label = await _authService.GetRole(userName);
                    Response.Cookies.Append("role", label, new CookieOptions
                    {
                        Expires = DateTime.Now.AddYears(10)
                    });
                    if (label == LabelCommon.Customer)
                    {
                        // Sản phẩm gợi ý
                        ViewData["recommendedProducts"] = await _productService.RecommendedProducts(pKeyword);
                        return View();
                    }
                }
            }

            // Chưa đăng nhập thì sản phẩm bình thường
            ViewData["recommendedProducts"] = await _productService.GetList(pKeyword);
            return View();
        }

        public async Task<IActionResult> Products([FromQuery] string? pKeyword = "", int? pPage = 1)
        {
            int pLimit = 5;
            ViewData["listProduct"] = await _productService.GetList(pKeyword, pPage, pLimit);
            ViewData["totalPages"] = await _productService.CalculateTotalPages(pLimit);
            return View();
        }
    }
}
