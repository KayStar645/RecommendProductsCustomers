using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;
using System.Text.Json;

namespace RecommendProductsCustomers.Services
{
    public class CustomerService : ICustomerService
	{
		BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
														   SettingCommon.Connect("UserName"),
														   SettingCommon.Connect("Password"));
        public async Task<List<CustomerModel>> GetList(string? pKeyword = "", int? pPage = 1)
        {
			var listJObject = await Repo.Get(LabelCommon.Customer, null, "", "", null, 100, pKeyword, pPage);

			var customers = listJObject.Select((JObject jObject) =>
			{
				var customer = new CustomerModel()
				{
					phone = jObject.Value<string>("phone"),
					name = jObject.Value<string>("name"),
                    gender = jObject.Value<string>("gender"),
                    dateBirth = string.IsNullOrEmpty(jObject.Value<string>("dateBirth")) ? null : DateTime.Parse(jObject.Value<string>("dateBirth")),
					address = jObject.Value<string>("address"),
				};

                // Lấy thêm cái sở thích bỏ vô

				return customer;
			}).ToList();

			return customers;
		}

        public async Task<bool> Create(CustomerModel pCustomer)
        {
            try
            {
                
                if (pCustomer != null)
                {
                    if(pCustomer.password != pCustomer.rePassword)
                    {
                        return false;
                    }    

                    string dateBirth = pCustomer.dateBirth == DateTime.Parse("1870-01-01") ? "" :
                        DateTime.Parse(pCustomer.dateBirth.ToString()).ToString("yyyy-MM-dd");

                    string? password = pCustomer.password;
                    JObject pObject = new JObject()
                    {
                        {"gender", pCustomer.gender },
                        {"phone", pCustomer.phone },
                        {"password", password },
                        {"name", pCustomer.name },
                        {"dateBirth", dateBirth },
                        {"address", pCustomer.address }
                    };
                    await Repo.Add(LabelCommon.Customer, pObject);

                    // Tự động thêm 1 user tương ứng với khách hàng này
                    JObject pUser = new JObject() {
                        { "userName", pCustomer.phone },
                        { "password", pCustomer.password }
                    };
                    await Repo.Add(LabelCommon.User, pUser);

                    // Khách hàng sở hữu tài khoản
                    await Repo.CreateRelationShip(LabelCommon.Customer, pObject, LabelCommon.User, pUser,
                        RelaCommon.Customer_User, null, true);

                    List<string> hobbies = JsonSerializer.Deserialize<List<string>>(pCustomer.hobbies.FirstOrDefault());
                    foreach(string hoppy in hobbies)
                    {
                        JObject whereHobbies = new JObject() 
                        {
                            { "name", hoppy },
                        };
                        // Khách hàng có sở thích
                        await Repo.CreateRelationShip(LabelCommon.Customer, pObject, LabelCommon.Hobby, whereHobbies,
                            RelaCommon.Customer_Hobby, null, true);
                    }    

                    // Thêm những sở thích của khách hàng này
                    // For qua tạo mối quan hệ thôi
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
                await Repo.Delete(LabelCommon.Customer, pId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CustomerModel> GetDetailByUserName(string pUserName)
        {
            try
            {
                JObject user = new JObject()
                {
                    {"userName", pUserName }
                };

                List<JObject> customers = await Repo.Get(LabelCommon.Customer, null, "own", LabelCommon.User, user, 1);

                var customer = customers.FirstOrDefault();
                CustomerModel finUser = new CustomerModel()
                {
                    id = customer.Value<string>("id"),
                    gender = customer.Value<string>("gender"),
                    phone = customer.Value<string>("phone"),
                    name = customer.Value<string>("name"),
                    dateBirth = string.IsNullOrEmpty(customer.Value<string>("dateBirth")) ? null : DateTime.Parse(customer.Value<string>("dateBirth")),
                    address = customer.Value<string>("address"),
                };
                return finUser;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Update(CustomerModel pCustomer)
        {
            try
            {
                if (pCustomer != null)
                {
                    string dateBirth = pCustomer.dateBirth == DateTime.Parse("1870-01-01") ? "" :
                        DateTime.Parse(pCustomer.dateBirth.ToString()).ToString("yyyy-MM-dd");
                    JObject pObject = new JObject()
                    {
                        {"gender", pCustomer.gender },
                        {"phone", pCustomer.phone },
                        {"name", pCustomer.name },
                        {"dateBirth", dateBirth },
                        {"address", pCustomer.address }
                    };
                    await Repo.Update(LabelCommon.Customer, pCustomer.id, pObject);
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
