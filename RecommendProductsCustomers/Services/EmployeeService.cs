using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Services
{
    public class EmployeeService : IEmployeeService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));
        public async Task<bool> Create(EmployeeModel pEmployee)
        {
            try
            {
                // Phải check internalCode duy nhất
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
                    await Repo.Add(LabelCommon.Employee, pObject);

                    // Tự động thêm 1 user tương ứng với nhân viên này
                    JObject pUser = new JObject() {
                        { "userName", pEmployee.internalCode },
                        { "password", pEmployee.internalCode }
                    };
                    await Repo.Add(LabelCommon.User, pUser);

                    // Nhân viên sở hữu tài khoản
                    await Repo.CreateRelationShip(LabelCommon.Employee, pObject, LabelCommon.User, pUser,
                        RelaCommon.Employee_User, null, true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string pId)
        {
            try
            {
                await Repo.Delete(LabelCommon.Employee, pId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<EmployeeModel> GetDetail(string pId)
        {
            throw new NotImplementedException();
        }

        public async Task<EmployeeModel> GetDetailByUserName(string pUserName)
        {
            try
            {
                JObject user = new JObject()
                {
                    {"userName", pUserName }
                };

                List<JObject> employees = await Repo.Get(LabelCommon.Employee, null, RelaCommon.Employee_User, LabelCommon.User, user, 1);

                var employee = employees.FirstOrDefault();
                EmployeeModel finUser = new EmployeeModel()
                {
                    id = employee.Value<string>("id"),
                    internalCode = employee.Value<string>("internalCode"),
                    name = employee.Value<string>("name"),
                    dateBirth = string.IsNullOrEmpty(employee.Value<string>("dateBirth")) ? null : DateTime.Parse(employee.Value<string>("dateBirth")),
                    gender = employee.Value<string>("gender"),
                    phone = employee.Value<string>("phone")
                };
                return finUser;
            }    
            catch
            {
                return null;
            }
        }

        public async Task<List<EmployeeModel>> GetList()
        {
            var listJObject = await Repo.Get(LabelCommon.Employee);

            var employees = listJObject.Select((JObject jObject) =>
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

            return employees;
        }

        public async Task<bool> Update(EmployeeModel pEmployee)
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
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
