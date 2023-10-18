using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IHobbyService
    {
        Task<List<HobbyModel>> GetList(string? pKeyword = "", int? pPage = 1);
        Task<bool> Create(HobbyModel pHobby);
    }
}
