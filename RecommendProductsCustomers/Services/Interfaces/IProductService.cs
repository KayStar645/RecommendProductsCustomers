using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductModel>> GetList();
        Task<ProductModel> GetDetailByUserName(string pUserName);
        Task<bool> Update(ProductModel pProduct);
    }
}
