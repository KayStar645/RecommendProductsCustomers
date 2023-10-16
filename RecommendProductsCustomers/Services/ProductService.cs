using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;

using System.Text.RegularExpressions;

namespace RecommendProductsProducts.Services
{
    public class ProductService : IProductService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));
        public async Task<List<ProductModel>> GetList()
        {
            var listJObject = await Repo.Get(LabelCommon.Product);

            var Products = listJObject.Select((JObject jObject) =>
            {
                var Product = new ProductModel()
                {
                    internalCode = jObject.Value<string>("internalCode"),
                    name = jObject.Value<string>("name"),
                    description = jObject["description"]?.Value<string>(),
                    size = jObject["size"]?.Value<string>(),
                    material = jObject["material"]?.Value<string>(),
                    preserve = jObject["preserve"]?.Value<string>(),
                    quantity = jObject["quantity"]?.Value<int>(),
                    price = jObject["price"]?.Value<long>()
                };
                string? str = jObject["images"]?.Value<string>();
                string cleanedInput = Regex.Replace(str, @"\s+", "").Trim('[', ']');

                Product.images = cleanedInput.Split(',')
                                                  .Select(url => url.Trim('\''))
                                                  .ToList();
                return Product;
            }).ToList();

            return Products;
        }


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

        public async Task<bool> Update(ProductModel pProduct)
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

                    if (pProduct.images != null && pProduct.images.Any())
                    {
                        string imageList = string.Join(",", pProduct.images);
                        updatedFields.Add("images", imageList);
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

    }
}
