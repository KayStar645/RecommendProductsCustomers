using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IImportBillService
    {
        Task<string> CreateInternalCode();
        Task<bool> CreateOrUpdate(ImportBillModel pImportBill);
    }
}
