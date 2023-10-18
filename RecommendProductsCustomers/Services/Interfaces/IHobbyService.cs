using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IHobbyService
    {
        Task<List<HobbyModel>> GetList(string? pRela = "", string? pLabelB = "", JObject? pWhereB = null, string? pKeyword = "", int? pPage = 1);
        Task<bool> Create(HobbyModel pHobby);
    }
}
