using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Models;
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
        public async Task<IActionResult> Index([FromQuery] string? pKeyword)
        {
            ViewData["listProduct"] = await _ProductService.GetList(pKeyword);
            return View();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProductModel pProduct)
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
