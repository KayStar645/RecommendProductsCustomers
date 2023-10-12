using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class ImportBillController : Controller
    {
        private readonly IImportBillService _importBillService;
        private readonly IEmployeeService _employeeService;

        public ImportBillController(IImportBillService importBillService, IEmployeeService employeeService)
        {
            _importBillService = importBillService;
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Detail([FromQuery] string pIdentity)
        {
            if (Request.Cookies.TryGetValue("userName", out string userName))
            { 
                ViewData["employee"] = await _employeeService.GetDetailByUserName(userName);
            }
            ViewData["internalCode"] = await _importBillService.CreateInternalCode();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] ImportBillVM pImportBillVM)
        {
            int a = 1;
            return RedirectToAction("Detail", pImportBillVM?.importBill?.id);
        }
    }
}
