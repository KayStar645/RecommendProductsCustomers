using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class ImportBillController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ImportCommodity([FromForm] object pInput)
        {
            return View();
        }
    }
}
