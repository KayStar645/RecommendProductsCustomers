using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services;
using RecommendProductsCustomers.Services.Interfaces;
using System.Data;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService) 
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? pSearch = "")
        {
            ViewData["listEmployee"] = await _employeeService.GetList(pSearch);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeModel pEmployee)
        {
            bool fag = await _employeeService.Create(pEmployee);
            if(fag)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }    
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EmployeeModel pEmployee)
        {
            bool fag = await _employeeService.Update(pEmployee);
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
            bool fag = await _employeeService.Delete(pId);
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
