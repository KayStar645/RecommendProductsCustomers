using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Common;
using RecommendProductsCustomers.Repositories;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class BackupRestoreController : Controller
    {
        BaseRepository Repo = new BaseRepository(SettingCommon.Connect("Uri"),
                                                           SettingCommon.Connect("UserName"),
                                                           SettingCommon.Connect("Password"));

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> Backup()
        {
            string backupPath = "wwwroot/db/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_backup.graphtml";
            await Repo.BackupDatabase(backupPath);
            return RedirectToAction("Index_Home", "_Home");
        }

        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> Restore()
        {
            string backupPath = "wwwroot/db/20231018101611_backup.graphtml";
            await Repo.RestoreDatabase(backupPath);
            return RedirectToAction("Index_Home", "_Home");
        }
    }
}
