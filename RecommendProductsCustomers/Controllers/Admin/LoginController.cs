using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService) 
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserModel pUser)
        {
            bool flag = await _authService.Login(pUser);

            if(flag)
            {
                Response.Cookies.Append("userName", pUser.userName, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(10)
                });

                string label = await _authService.GetRole(pUser.userName);
                Response.Cookies.Append("role", label, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(10)
                });
                if (label == LabelCommon.Employee)
                {
                    return RedirectToAction("Index_Home", "_Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }    
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete("userName");

            return RedirectToAction("Login", "Login");
        }
    }
}
