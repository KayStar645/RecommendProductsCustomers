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
            string backupPath = "wwwroot/db/" + Guid.NewGuid().ToString() + "backup.graphml";
            await Repo.BackupDatabase(backupPath);
            return RedirectToAction("Index_Home", "_Home");
        }

        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> Restore()
        {
            string backupPath = "wwwroot/db/24554800-d47a-44c1-bf2e-f306eb6a31d0backup.graphml";
            await Repo.RestoreDatabase(backupPath);
            return RedirectToAction("Index_Home", "_Home");
        }
    }
}
