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
                    Response.Cookies.Append("userName", userName, new CookieOptions
                    {
                        Expires = DateTime.Now.AddYears(10)
                    });

                    string label = await _authService.GetRole(userName);
                    Response.Cookies.Append("role", label, new CookieOptions
                    {
                        Expires = DateTime.Now.AddYears(10)
                    });
                    if (label == LabelCommon.Employee)
                    {
                        return View();
                    }
                }
            }
            return RedirectToAction("Login", "Login");
        }
    }
}
