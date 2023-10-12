using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Models;
using RecommendProductsCustomers.Repositories;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Services
{
    public class ImportBillService : IImportBillService
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));

        public async Task<string> CreateInternalCode()
        {
            string max = await Repo.Max(LabelCommon.ImportBill, "internalCode");
            if(max != null)
            {
                string value = (int.Parse(max.Substring(2, max.Length - 1)) + 1).ToString();
                while(value.Length < 7)
                {
                    value = "0" + value;
                }    
                return "MDN" + value;
            }
            return "MDN0000001";
        }

        public async Task<bool> CreateOrUpdate(ImportBillModel pImportBill)
        {
            // Nếu id != null thì tìm theo id
            // Nếu tìm thấy thì update
            // Ngược lại thêm mới
            return true;
        }    
    }
}
