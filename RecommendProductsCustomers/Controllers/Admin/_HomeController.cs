using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class _HomeController : Controller
    {
        private readonly IAuthService _authService;

        public _HomeController(IAuthService authService) 
        {
            _authService = authService;
        }
        public async Task<IActionResult> Index_Home()
        {
            if (Request.Cookies.TryGetValue("userName", out string userName))
            {
                bool flag = await _authService.Login(new UserModel() { userName = userName, password = userName });

                if (flag)
                {
                    return View();
                }
            }
            return RedirectToAction("Login", "Login");
        }
    }
}
