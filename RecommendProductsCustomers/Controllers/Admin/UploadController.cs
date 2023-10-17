using Microsoft.AspNetCore.Mvc;
using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Controllers.Admin
{
    public class UploadController : Controller
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost()]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var imageUrl = await _uploadService.UploadImage(file);

            if (imageUrl != null)
            {
                return Ok(new { imageUrl });
            }
            else
            {
                return BadRequest("Invalid file");
            }
        }

    }
}
