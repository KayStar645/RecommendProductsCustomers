using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;
using RecommendProductsCustomers.Services.Interfaces;


namespace RecommendProductsProducts.Controllers.Admin
{
    public class ProductController : Controller
    {
        private readonly IProductService _ProductService;

        public ProductController(IProductService ProductService)
        {
            _ProductService = ProductService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? pSearch)
        {
            ViewData["listProduct"] = await _ProductService.GetList(pSearch);
            return View();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProductVM pProduct)
        {
            bool fag = await _ProductService.Update(pProduct);
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
