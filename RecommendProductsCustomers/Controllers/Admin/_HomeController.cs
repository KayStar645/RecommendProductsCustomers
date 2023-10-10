using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class _HomeController : Controller
    {
        public IActionResult Index_Home()
        {
            return View();
        }
    }
}
