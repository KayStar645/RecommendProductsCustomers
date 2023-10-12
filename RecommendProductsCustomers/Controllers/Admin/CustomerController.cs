using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Models;
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerModel pCustomer)
        {
            bool fag = await _customerService.Create(pCustomer);
            if (fag)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CustomerModel pCustomer)
        {
            bool fag = await _customerService.Update(pCustomer);
            if (fag)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string pId)
        {
            bool fag = await _customerService.Delete(pId);
            if (fag)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


    }
}
