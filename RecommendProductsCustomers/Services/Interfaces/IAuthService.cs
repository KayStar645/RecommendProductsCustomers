using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> Login(UserModel pUser);
        Task<string> GetRole(string pInternalCode);
    }
}
