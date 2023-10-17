using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;
using System.Text.RegularExpressions;

namespace RecommendProductsCustomers.Services
{
    public class ProductService : IProductService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));

        public Task<ProductModel> GetDetail(string pIdentity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ProductModel>> GetList(string? keyword = "")
        {
            var listJObject = await Repo.Get(LabelCommon.Product);

            var products = listJObject.Select(p =>
            {
                var productModel = new ProductModel()
                {
                    id = p["id"]?.Value<string>(),
                    internalCode = p["internalCode"]?.Value<string>(),
                    name = p["name"]?.Value<string>(),
                    description = p["description"]?.Value<string>(),
                    size = p["size"]?.Value<string>(),
                    material = p["material"]?.Value<string>(),
                    preserve = p["preserve"]?.Value<string>(),
                    quantity = p["quantity"]?.Value<int>(),
                    price = p["price"]?.Value<long>()
                };

                string? str = p["images"]?.Value<string>();
                string cleanedInput = Regex.Replace(str, @"\s+", "").Trim('[', ']');

                productModel.images = cleanedInput.Split(',')
                                                  .Select(url => url.Trim('\''))
                                                  .ToList();

                return productModel;
            }).ToList();

            return products;
        }
    }
}
