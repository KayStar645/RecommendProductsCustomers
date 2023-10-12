using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Services;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
	public class CustomerController : Controller
	{
		private readonly ICustomerService _customerService;

		public CustomerController(ICustomerService customerService)
		{
			_customerService = customerService;
		}
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			ViewData["listCustomer"] = await _customerService.GetList();
			return View();
		}
		
	}
}
