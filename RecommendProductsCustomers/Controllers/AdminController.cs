using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
