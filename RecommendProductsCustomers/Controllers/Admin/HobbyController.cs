using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class HobbyController : Controller
    {
        private readonly IHobbyService _hobbyService;

        public HobbyController(IHobbyService hobbyService)
        {
            _hobbyService = hobbyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery]string? pSearch = "")
        {
           ViewData["listHobbies"] = await _hobbyService.GetList(pSearch);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HobbyModel pHobbyModel)
        {
            bool flag = await _hobbyService.Create(pHobbyModel);
            if (flag)
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
