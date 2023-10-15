using RecommendProductsCustomers.Models;

namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> Login(UserModel pUser);
    }
}
