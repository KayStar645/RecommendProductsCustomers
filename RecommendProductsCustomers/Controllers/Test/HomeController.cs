using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Repositories;

namespace RecommendProductsCustomers.Controllers.Test
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

            //List<JObject> peoples = await Repo.Get(LabelCommon.People);

            //ViewBag.Peoples = peoples;

            //var jsonPeople = new JObject()
            //{
            //    {"id", "1" },
            //    {"name", "Phạm Tấn Tài" },
            //    {"age", "19" }
            //};

            //JObject newPeople = await Repo.Add(LabelCommon.People, jsonPeople);


            //ViewBag.NewPeople = newPeople;

            //var where = new JObject()
            //{
            //    {"name", "Phạm Tấn Thuận" },
            //};

            //var value = new JObject()
            //{
            //    {"name", "Thuận" },
            //    {"age", "21" },
            //    {"class", "11DHTH4" },
            //};

            //List<JObject> updatePeople = await Repo.Update(LabelCommon.People, where, value);

            //ViewBag.UpdatePeople = updatePeople;

            //var delete = new JObject()
            //{
            //    {"name", "Phạm Tấn Thuận" },
            //};

            //int countDelete = await Repo.Delete(LabelCommon.People, delete);

            //ViewBag.CountDelete = countDelete;

            // RelationShip
            var A = new JObject()
            {
                {"id", "1" },
                {"name", "Phạm Tấn Thuận" },
                {"age", "19" }
            };

            var B = new JObject()
            {
                {"id", "1" },
                {"name", "Phạm Tấn Tài" },
                {"age", "19" }
            };

            //await Repo.CreateRelationShip(LabelCommon.People, A, LabelCommon.People, B, "xxx", null, false);
            //await Repo.DeleteRelationShip(LabelCommon.People, B, "ChuNo", false);

            var update = new JObject()
            {
                {"since", "2015" }
            };

            await Repo.UpdateRelationShip(LabelCommon.People, A, "friend", LabelCommon.People, B, update, true, false);
            return View();
        }
    }
}