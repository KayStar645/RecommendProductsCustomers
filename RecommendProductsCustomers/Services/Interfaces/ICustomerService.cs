using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
	public interface ICustomerService
	{
		Task<List<CustomerModel>> GetList();
        Task<CustomerModel> GetDetailByUserName(string pUserName);
        Task<bool> Create(CustomerModel pCustomer);
        Task<bool> Update(CustomerModel pCustomer);
        Task<bool> Delete(string id);
    }
}
