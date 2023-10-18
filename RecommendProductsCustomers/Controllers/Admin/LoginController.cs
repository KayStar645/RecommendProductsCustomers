using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IHobbyService _hobbyService;
        private readonly ICustomerService _customerService;

        public LoginController(IAuthService authService, IHobbyService hobbyService, ICustomerService customerService) 
        {
            _authService = authService;
            _hobbyService = hobbyService;
            _customerService = customerService;
        }

        [HttpGet()]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost()]
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

        [HttpGet("Registration")]
        public async Task<IActionResult> Registration()
        {
            ViewData["listHobbies"] = await _hobbyService.GetList();
            return View();
        }

        [HttpPost()]
        public async Task<IActionResult> Registration([FromForm] CustomerModel pCustomer)
        {
            bool result = await _customerService.Create(pCustomer);

            if(result)
            {
                Response.Cookies.Append("userName", pCustomer.phone, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(10)
                });
                Response.Cookies.Append("role", LabelCommon.Customer, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(10)
                });
                return RedirectToAction("Index", "Home");
            }    

            return View();
        }


        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete("userName");
            HttpContext.Response.Cookies.Delete("role");

            return RedirectToAction("Login", "Login");
        }
    }
}
