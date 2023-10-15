using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeModel>> GetList();
        Task<EmployeeModel> GetDetail(string pId);
        Task<EmployeeModel> GetDetailByUserName(string pUserName);
        Task<bool> Create(EmployeeModel pEmployee);
        Task<bool> Update(EmployeeModel pEmployee);
        Task<bool> Delete(string id);
    }
}
