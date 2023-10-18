using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductModel> GetDetailByUserName(string pUserName);

        Task<bool> Update(UpdateProductVM pProduct);
        Task<List<ProductModel>> GetList(string? pKeyword = "");

        Task<bool> Update(ProductModel pProduct);
        Task<List<ProductModel>> GetList(string? pKeyword = "", int? pPage = 1);

        Task<ProductModel> GetDetail(string pIdentity);
    }
}
