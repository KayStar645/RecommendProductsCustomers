namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IImportBillService
    {
        Task<string> CreateInternalCode();
    }
}
