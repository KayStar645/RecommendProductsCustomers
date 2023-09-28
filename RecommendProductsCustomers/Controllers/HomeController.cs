using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Repositories;

namespace RecommendProductsCustomers.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));

            List<JObject> peoples = await Repo.Get(LabelCommon.People);

            var jsonPeople = new JObject()
            {
                {"id", "1" },
                {"name", "Phạm Tấn Thuận" },
                {"age", "19" }
            };

            JObject newPeople = await Repo.Add(LabelCommon.People, jsonPeople);

            ViewBag.Peoples = peoples;

            ViewBag.NewPeople = newPeople;

            return View();
        }
    }
}