using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Services
{
    public class AuthService : IAuthService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));
        public async Task<bool> Login(UserModel pUser)
        {
            try
            {
                JObject user = new JObject()
                {
                    {"userName", pUser.userName },
                    {"password", pUser.password },
                };

                List<JObject> findUser = await Repo.Get(LabelCommon.User, user);
                if (findUser.Count > 0)
                {
                    return true;
                }
                return false;
            }    
            catch
            {
                return false;
            }
        }
    }
}
