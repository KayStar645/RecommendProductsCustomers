using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Controllers.Customer
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
