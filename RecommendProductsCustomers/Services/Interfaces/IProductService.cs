using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Models.ViewModel;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductModel> GetDetailByUserName(string pUserName);

        Task<bool> Update(UpdateProductVM pProduct);
        Task<List<ProductModel>> GetList(string? pKeyword = "", int? pPage = 1, int? pLimit = 100);
        Task<List<ProductModel>> RecommendedProducts(string pUserName, string? pKeyword = "", int? pPage = 1, int? pLimit = 100);

        Task<ProductModel> GetDetail(string pIdentity);
        Task<int> CalculateTotalPages(string pKeyword, int itemsPerPage);
    }
}
