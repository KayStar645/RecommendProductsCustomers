using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;
using RecommendProductsCustomers.Services.Interfaces;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Reflection.Emit;
using RecommendProductsCustomers.Common;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class ImportBillController : Controller
    {
        private readonly IImportBillService _importBillService;
        private readonly IEmployeeService _employeeService;
        private readonly IHobbyService _hobbyService;

        public ImportBillController(IImportBillService importBillService, IEmployeeService employeeService, IHobbyService hobbyService)
        {
            _importBillService = importBillService;
            _employeeService = employeeService;
            _hobbyService = hobbyService;
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
            ViewData["listHobbies"] = await _hobbyService.GetList();
            if (pIdentity == null)
            {
                ViewData["internalCode"] = await _importBillService.CreateInternalCode();
                ViewData["importDate"] = DateTime.Now.ToString("dd/MM/yyyy");

                if (Request.Cookies.TryGetValue("userName", out string userName))
                {
                    ViewData["employee"] = await _employeeService.GetDetailByUserName(userName);
                }
            }
            else
            {
                List<(ImportBillModel, EmployeeModel, List<ProductModel>)> results = await _importBillService.Get(pIdentity);
                List<ProductModel> products = new List<ProductModel>();

                ViewData["importBill"] = results[0].Item1;
                ViewData["employee"] = results[0].Item2;
                products = results[0].Item3;
                foreach(ProductModel model in products)
                {
                    List<HobbyModel> hobbies = new List<HobbyModel>();
                    JObject where = new JObject
                    {
                        {"internalCode", model.internalCode }
                    };
                    hobbies = await _hobbyService.GetList(RelaCommon.Product_Hobbies, LabelCommon.Product, where);
                    model.hobbies = hobbies.Select(x => x.name).ToList();
                }

                ViewData["products"] = products;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] ImportBillVM pImportBillVM)
        {
            Request.Cookies.TryGetValue("userName", out string userName);
            EmployeeModel employee = await _employeeService.GetDetailByUserName(userName);

            await _importBillService.CreateOrUpdate(employee, pImportBillVM.importBill, pImportBillVM.products);

            return RedirectToAction("Index");
        }
    }
}
