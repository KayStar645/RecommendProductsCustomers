using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Services
{
    public class HobbyService : IHobbyService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));

        public async Task<List<HobbyModel>> GetList(string? pRela = "", string? pLabelB = "", JObject? pWhereB = null, string? pKeyword = "", int? pPage = 1)
        {
            var listJObject = await Repo.Get(LabelCommon.Hobby, null, pRela, pLabelB, pWhereB, 100, pKeyword, pPage);

            var interests = listJObject.Select(p =>
            {
                var interestModel = new HobbyModel()
                {
                    id = p["id"]?.Value<string>(),
                    name = p["name"]?.Value<string>(),
                    image = p["image"]?.Value<string>()
                };

                return interestModel;
            }).ToList();

            return interests;
        }

        public async Task<bool> Create(HobbyModel pHobby)
        {
            try
            {
                if (pHobby != null)
                {
                    JObject pObject = new JObject()
                    {
                        {"name", pHobby.name },
                        {"image", pHobby.image },
                    };
                    await Repo.Add(LabelCommon.Hobby, pObject);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
