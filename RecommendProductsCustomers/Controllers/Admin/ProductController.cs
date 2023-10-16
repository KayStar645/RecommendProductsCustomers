using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Services.Interfaces;
using RecommendProductsProducts.Services;


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
        public async Task<IActionResult> Index()
        {
            ViewData["listProduct"] = await _ProductService.GetList();
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
