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

        public async Task<IActionResult> Index()
        {
            List<(ImportBillModel, EmployeeModel, List<ProductModel>)> results = await _importBillService.Get();

            List<ImportBillModel> importBills = new List<ImportBillModel>();
            List<EmployeeModel> employees = new List<EmployeeModel>();
            List<List<ProductModel>> products = new List<List<ProductModel>>();

            foreach (var result in results)
            {
                importBills.Add(result.Item1);
                employees.Add(result.Item2);
                products.Add(result.Item3);
            }

            ViewData["importBill"] = importBills;
            ViewData["employee"] = employees;
            ViewData["products"] = products;

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
            Request.Cookies.TryGetValue("userName", out string userName);
            EmployeeModel employee = await _employeeService.GetDetailByUserName(userName);

            await _importBillService.CreateOrUpdate(employee, pImportBillVM.importBill, pImportBillVM.products);



            return RedirectToAction("Detail", new { pImportBillVM?.importBill?.id });
        }
    }
}
