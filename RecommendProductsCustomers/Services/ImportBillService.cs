using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;
using System.Reflection.Emit;

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
                string value = (int.Parse(max.Substring(2, max.Length - 1)) + 1).ToString();
                while(value.Length < 7)
                {
                    value = "0" + value;
                }    
                return "MDN" + value;
            }
            return "MDN0000001";
        }

        public T ExtractModel<T>(JToken token) where T : class, new()
        {
            if (token != null)
            {
                return token.ToObject<T>();
            }

            return new T();
        }

        public async Task Get(string? id = null)
        {
            string query = "";
            if (id == null)
            {
                query = $"match (a:ImportBill), (b:Employee), (c:Product), (a)-[x:import]-(b), (a)-[y:have]-(c) " +
                        $"return *";
            }
            else
            {
                query = $"match (a:ImportBill), (b:Employee), (c:Product), (a)-[x:import]-(b), (a)-[y:have]-(c) " +
                        $"where id(a) = {id} " +
                        $"return *";
            }
            var result = await Repo.GetQuery(query);
            if (result.Any())
            {
                var list = result.Select(record =>
                {
                    ImportBillModel importBill = ExtractModel<ImportBillModel>(record["a"]);
                    EmployeeModel employee = ExtractModel<EmployeeModel>(record["b"]);
                    ProductModel product = ExtractModel<ProductModel>(record["c"]);

                    return (importBill, employee, product);
                }).ToList();

            }
        }

        public async Task<bool> CreateOrUpdate(EmployeeModel pEmployee, ImportBillModel pImportBill, List<ProductModel> pProducts)
        {
            if (pProducts.Count == 0)
            {
                // Đơn hàng bắt buộc phải có sản phẩm
                return false;
            }

            // Tìm hóa đơn có internalCode
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

                // Cập nhật lại bill này
                bill = (await Repo.Update(LabelCommon.ImportBill, whereBill, billObject)).First();


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


                // Tạo mối quan hệ xác định nhân viên lập đơn hàng - OK
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
            if(isCreate == false)
            {
                // - Chưa vô
                await Repo.DeleteRelationShipNotExists(LabelCommon.ImportBill, bill["id"].ToString(),
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
                    { "quantity", product?.quantity?.ToString() },
                    { "price", product?.price?.ToString() }
                };

                if (findProduct == null)
                {
                    // Tạo mới product
                    await Repo.Add(LabelCommon.Product, jObjectProduct);
                }
                else
                {
                    await Repo.Update(LabelCommon.Product, whereProduct, jObjectProduct);
                }

                // Tìm relationShip của sản phẩm có internalCode với bill có id
                JObject findRela = await Repo.GetRelationShip(LabelCommon.ImportBill, bill["id"].ToString(),
                                                              RelaCommon.ImportBill_Product, LabelCommon.Product, whereProduct);

                if(findRela == null)
                {
                    JObject findwhereBill = new JObject()
                    {
                        {"internalCode", bill["internalCode"] }
                    };

                    JObject findwhereProduct = new JObject()
                    {
                        {"internalCode", product.internalCode }
                    };

                    await Repo.CreateRelationShip(LabelCommon.ImportBill, findwhereBill, LabelCommon.Product,
                                                  findwhereProduct, RelaCommon.ImportBill_Product);

                }
            }
            return true;
        }    
    }
}
