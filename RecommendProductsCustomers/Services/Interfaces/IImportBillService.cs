using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IImportBillService
    {
        Task<string> CreateInternalCode();
        Task<List<(ImportBillModel, EmployeeModel, List<ProductModel>)>> Get(string? id = null);
        Task<bool> CreateOrUpdate(EmployeeModel pEmployee, ImportBillModel pImportBill, List<ProductModel> pProducts);
    }
}
