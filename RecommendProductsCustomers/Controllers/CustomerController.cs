using Microsoft.AspNetCore.Mvc;

namespace RecommendProductsCustomers.Controllers
{
	public class CustomerController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
