using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductModel>> GetList(string? pKeyword = "");
        Task<ProductModel> GetDetail(string pIdentity);
    }
}
