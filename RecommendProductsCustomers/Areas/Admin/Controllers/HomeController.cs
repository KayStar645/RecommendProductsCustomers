using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Areas.Admin.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
