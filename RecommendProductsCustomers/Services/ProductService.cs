using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;
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
        public async Task<ProductModel> GetDetailByUserName(string pUserName)
        {
            try
            {
                JObject user = new JObject()
                {
                    {"userName", pUserName }
                };

                List<JObject> Products = await Repo.Get(LabelCommon.Product, null, "own", LabelCommon.User, user, 1);

                var Product = Products.FirstOrDefault();
                ProductModel finUser = new ProductModel()
                {
                    id = Product.Value<string>("id"),
                    internalCode = Product.Value<string>("internalCode"),
                    name = Product.Value<string>("name"),
                    description = Product["description"]?.Value<string>(),
                    size = Product["size"]?.Value<string>(),
                    material = Product["material"]?.Value<string>(),
                    preserve = Product["preserve"]?.Value<string>(),
                    quantity = Product["quantity"]?.Value<int>(),
                    price = Product["price"]?.Value<long>()
                };
                string? str = Product["images"]?.Value<string>();
                string cleanedInput = Regex.Replace(str, @"\s+", "").Trim('[', ']');

                finUser.images = cleanedInput.Split(',')
                                                  .Select(url => url.Trim('\''))
                                                  .ToList();
                return finUser;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Update(UpdateProductVM pProduct)
        {
            try
            {
                if (pProduct != null)
                {
                    JObject updatedFields = new JObject();

                    if (pProduct.price.HasValue)
                    {
                        updatedFields.Add("price", pProduct.price);
                    }

                    // Kiểm tra xem có bất kỳ trường nào cần cập nhật không
                    if (updatedFields.HasValues)
                    {
                        await Repo.Update(LabelCommon.Product, pProduct.id, updatedFields);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public Task<ProductModel> GetDetail(string pIdentity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ProductModel>> GetList(string? pKeyword = "", int? pPage = 1, int? pLimit = 100)
        {
            var listJObject = await Repo.Get(LabelCommon.Product, null, "", "", null, pLimit, pKeyword, pPage);

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

        public async Task<int> CalculateTotalPages(string pKeyword, int itemsPerPage)
        {
            return await Repo.CalculateTotalPages(LabelCommon.Product, pKeyword, itemsPerPage);
        }

        public async Task<List<ProductModel>> RecommendedProducts(string pUserName, string? pKeyword = "", int? pPage = 1, int? pLimit = 100)
        {
            string query = $"MATCH (a:Product), (b:Customer {{phone:\"{pUserName}\"}}), (c:Hobby) " +
                           $"WHERE (b)-[:have]-(c) AND (c)-[:fit]-(a) " +
                           $"RETURN DISTINCT a";
            var result = await Repo.GetQuery(query);

            var JObjects = result.Select(record =>
            {
                var a = record["a"].As<INode>();

                var products = new JObject();
                products.Add("id", JToken.FromObject(a.Id.ToString()));
                foreach (var pair in a.Properties)
                {
                    products.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                return products;
            }).ToList();

            var products = JObjects.Select((JObject p) =>
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
                if (str != null)
                {
                    string cleanedInput = Regex.Replace(str, @"\s+", "").Trim('[', ']');
                    productModel.images = cleanedInput.Split(',')
                                                      .Select(url => url.Trim('\''))
                                                      .ToList();
                }
                else
                {
                    productModel.images = new List<string>();
                }

                return productModel;
            }).ToList();

            return products;
        }
    }
}
