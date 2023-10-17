using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;
using System.Text.RegularExpressions;

namespace RecommendProductsCustomers.Services
{
    public class ImportBillService : IImportBillService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));

        public async Task<string> CreateInternalCode()
        {
            string max = await Repo.Max(LabelCommon.ImportBill, "internalCode");
            if(max != null)
            {
                string value = (int.Parse(max.Substring(3, max.Length - 3)) + 1).ToString();
                while(value.Length < 7)
                {
                    value = "0" + value;
                }    
                return "MDN" + value;
            }
            return "MDN0000001";
        }

        public async Task<List<(ImportBillModel, EmployeeModel, List<ProductModel>)>> Get(string? id = null)
        {
            string query = "";
            if (id == null)
            {
                query = $"match (a:ImportBill), (b:Employee), (c:Product), (a)-[x:import]-(b), (a)-[y:have]-(c) " +
                        $"return a, b, c";
            }
            else
            {
                query = $"match (a:ImportBill), (b:Employee), (c:Product), (a)-[x:import]-(b), (a)-[y:have]-(c) " +
                        $"where id(a) = {id} " +
                        $"return a, b, c";
            }
            var result = await Repo.GetQuery(query);
            var list = result.Select(record =>
            {
                var a = record["a"].As<INode>();
                var b = record["b"].As<INode>();
                var c = record["c"].As<INode>();

                var importBill = new JObject();
                importBill.Add("id", JToken.FromObject(a.Id.ToString()));
                foreach (var pair in a.Properties)
                {
                    importBill.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                var employee = new JObject();
                employee.Add("id", JToken.FromObject(b.Id.ToString()));
                foreach (var pair in b.Properties)
                {
                    employee.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                var product = new List<JObject>();
                var productObject = new JObject();
                productObject.Add("id", JToken.FromObject(c.Id.ToString()));
                foreach (var pair in c.Properties)
                {
                    productObject.Add(pair.Key, JToken.FromObject(pair.Value));
                }
                product.Add(productObject);


                return (importBill, employee, product);
            })
            .Select(tuple =>
            {
                var importBillModel = tuple.importBill.ToObject<ImportBillModel>();
                var employeeModel = tuple.employee.ToObject<EmployeeModel>();
                var productModels = tuple.product.Select(p =>
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

                return (importBillModel, employeeModel, productModels);
            })
            .ToList();

            return list;
        }



        public async Task<bool> CreateOrUpdate(EmployeeModel pEmployee, ImportBillModel pImportBill, List<ProductModel> pProducts)
        {
            if (pProducts.Count == 0)
            {
                return false;
            }

            JObject whereBill = new JObject()
            {
                {"internalCode", pImportBill.internalCode }
            };  
            JObject? billObject = (await Repo.Get(LabelCommon.ImportBill, whereBill)).FirstOrDefault();

            bool isCreate = true;
            JObject bill = new JObject();
            if (billObject != null)
            {
                isCreate = false;
                if (billObject["isActive"] != null && (bool)billObject["isActive"])
                {
                    // Đơn hàng này đã nhập vào kho, không thể cập nhật
                    return false;
                }

                // Cập nhật lại bill này nè: pImportBill
                JObject jObjectBill = new JObject
                {
                    { "internalCode", pImportBill.internalCode },
                    { "dateImport", DateTime.Parse(pImportBill.dateImport.ToString()).ToString("yyyy-MM-dd") },
                    { "distributor", pImportBill.distributor },
                    { "contactPhone", pImportBill.contactPhone },
                    { "price", pImportBill.price?.ToString() },
                    { "isActive", pImportBill.isActive?.ToString() }
                };
                bill = (await Repo.Update(LabelCommon.ImportBill, whereBill, jObjectBill)).First();

                // Tìm nhân viên lập hóa đơn này trước đó
                EmployeeModel employeeImport = (await Repo.Get(LabelCommon.Employee, null, RelaCommon.Employee_ImportBill,
                                                              LabelCommon.ImportBill, bill)).First()
                                                              .ToObject<EmployeeModel>();
                if(employeeImport?.internalCode != pEmployee.internalCode)
                {
                    // Cập nhật lại nhân viên lập đơn hàng này
                    // Xóa Rela cũ
                    await Repo.DeleteRelationShip(LabelCommon.Employee, JObject.FromObject(employeeImport), RelaCommon.Employee_ImportBill);
                    // Tạo Rela mới
                    await Repo.CreateRelationShip(LabelCommon.Employee, JObject.FromObject(pEmployee),
                                              LabelCommon.ImportBill, bill, RelaCommon.Employee_ImportBill);
                }
            }    
            else
            {
                // Tạo mới bill
                JObject jObjectBill = new JObject
                {
                    { "internalCode", pImportBill.internalCode },
                    { "dateImport", DateTime.Parse(pImportBill.dateImport.ToString()).ToString("yyyy-MM-dd") },
                    { "distributor", pImportBill.distributor },
                    { "contactPhone", pImportBill.contactPhone },
                    { "price", pImportBill.price?.ToString() },
                    { "isActive", pImportBill.isActive?.ToString() }
                };
                bill = await Repo.Add(LabelCommon.ImportBill, jObjectBill);

                // Tạo mối quan hệ xác định nhân viên lập đơn hàng
                JObject jObjectRela = new JObject
                {
                    { "internalCode", pEmployee.internalCode }
                };
                JObject jBillRela = new JObject
                {
                    { "internalCode", bill["internalCode"] }
                };
                await Repo.CreateRelationShip(LabelCommon.Employee, jObjectRela,
                                              LabelCommon.ImportBill, jBillRela, RelaCommon.Employee_ImportBill);
            }

            // Xóa những Rela của sản phẩm không có trong danh sách cập nhật mà lại có trong db
            var x = pProducts.Select(product => product.internalCode).ToList();
            if (isCreate == false)
            {
                await Repo.DeleteRelationShipNotExists(LabelCommon.ImportBill, pImportBill.id.ToString(),
                                                        LabelCommon.Product, RelaCommon.ImportBill_Product,
                                                        "internalCode", pProducts.Select(product => product.internalCode).ToList());
            }    

            foreach(ProductModel product in pProducts)
            {
                JObject whereProduct = new JObject()
                {
                    {"internalCode", product.internalCode }
                };
                JObject findProduct = (await Repo.Get(LabelCommon.Product, whereProduct)).FirstOrDefault();
                JObject jObjectProduct = new JObject
                {
                    { "internalCode", product?.internalCode },
                    { "name", product?.name },
                    { "description", product?.description },
                    { "size", product?.size },
                    { "material", product?.material },
                    { "preserve", product?.preserve },
                    { "images", new JArray(product?.images) },
                };

                if (findProduct == null)
                {
                    if(pImportBill.isActive == true)
                    {
                        jObjectProduct.Add("quantity", product?.quantity?.ToString());
                        jObjectProduct.Add("price", ((int)(product?.price + product?.price * 0.1)).ToString());
                    }
                    else
                    {
                        jObjectProduct.Add("quantity", "0");
                        jObjectProduct.Add("price", ((int)(product?.price + product?.price * 0.1)).ToString());
                    }    
                    // Tạo mới product
                    await Repo.Add(LabelCommon.Product, jObjectProduct);
                }
                else
                {
                    if (pImportBill.isActive == true)
                    {
                        int? quantity = int.Parse(findProduct["quantity"].ToString()) + product?.quantity;
                        jObjectProduct.Add("quantity", quantity);
                        jObjectProduct.Add("price", (product?.price + product?.price * 0.1).ToString());
                    }
                    else
                    {
                        jObjectProduct.Add("quantity", "0");
                        jObjectProduct.Add("price", (product?.price + product?.price * 0.1).ToString());
                    }    
                    await Repo.Update(LabelCommon.Product, whereProduct, jObjectProduct);
                }

                // Tìm relationShip của sản phẩm có internalCode với bill có id
                JObject findRela = await Repo.GetRelationShip(LabelCommon.ImportBill, bill["id"].ToString(),
                                                              RelaCommon.ImportBill_Product, LabelCommon.Product, whereProduct);

                JObject findwhereBill = new JObject()
                    {
                        {"internalCode", bill["internalCode"] },
                    };
                JObject findwhereProduct = new JObject()
                    {
                        {"internalCode", product.internalCode }
                    };
                JObject proRela = new JObject
                    {
                        {"quantity", product?.quantity?.ToString() },
                        {"price", product?.price?.ToString() }
                    };
                if (findRela == null)
                {
                    await Repo.CreateRelationShip(LabelCommon.ImportBill, findwhereBill, LabelCommon.Product,
                                                  findwhereProduct, RelaCommon.ImportBill_Product, proRela);
                }
                else
                {
                    await Repo.UpdateRelationShip(LabelCommon.ImportBill, findwhereBill, RelaCommon.ImportBill_Product,
                                                  LabelCommon.Product, findwhereProduct, proRela);
                }
            }

            if (pImportBill.isActive == true)
            {
                JObject _new = new JObject
                {
                    {"isActive", "True" }
                };
                await Repo.Update(LabelCommon.ImportBill, whereBill, _new);
            }
            return true;
        }

        public async Task<bool> ImportBill(string pInternalCode)
        {
            try
            {
                JObject where = new JObject
                {
                    {"internalCode", pInternalCode }
                };
                JObject _new = new JObject
                {
                    {"isActive", "True" }
                };
                await Repo.Update(LabelCommon.ImportBill, where, _new);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
