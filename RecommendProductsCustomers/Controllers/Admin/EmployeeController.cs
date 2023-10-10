using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create()
        {
            return View();
        }
    }
}
