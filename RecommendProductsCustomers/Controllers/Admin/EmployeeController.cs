using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using System.Data;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class EmployeeController : Controller
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listJObject = await Repo.Get(LabelCommon.Employee);

                ViewData["listEmployee"] = listJObject.Select((JObject jObject) =>
                {
                    var employee = new EmployeeModel
                    {
                        id = jObject.Value<string>("id"),
                        internalCode = jObject.Value<string>("internalCode"),
                        name = jObject.Value<string>("name"),
                        dateBirth = string.IsNullOrEmpty(jObject.Value<string>("dateBirth")) ? null : DateTime.Parse(jObject.Value<string>("dateBirth")),
                        gender = jObject.Value<string>("gender"),
                        phone = jObject.Value<string>("phone")
                    };
                    return employee;
                }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeModel pEmployee)
        {
            try
            {
                if(pEmployee != null)
                {
                    string dateBirth = pEmployee.dateBirth == DateTime.Parse("1870-01-01") ? "" :
                        DateTime.Parse(pEmployee.dateBirth.ToString()).ToString("yyyy-MM-dd");
                    JObject pObject = new JObject()
                    {
                        {"internalCode", pEmployee.internalCode },
                        {"name", pEmployee.name },
                        {"gender", pEmployee.gender },
                        {"dateBirth", dateBirth },
                        {"phone", pEmployee.phone }
                    };
                    await Repo.Add(LabelCommon.Employee, pObject);
                }    
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EmployeeModel pEmployee)
        {
            try
            {
                if (pEmployee != null)
                {
                    string dateBirth = pEmployee.dateBirth == DateTime.Parse("1870-01-01") ? "" :
                        DateTime.Parse(pEmployee.dateBirth.ToString()).ToString("yyyy-MM-dd");
                    JObject pObject = new JObject()
                    {
                        {"internalCode", pEmployee.internalCode },
                        {"name", pEmployee.name },
                        {"gender", pEmployee.gender },
                        {"dateBirth", dateBirth },
                        {"phone", pEmployee.phone }
                    };
                    await Repo.Update(LabelCommon.Employee, pEmployee.id, pObject);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            await Repo.Delete(LabelCommon.Employee, id);
            return RedirectToAction("Index");
        }
    }
}
