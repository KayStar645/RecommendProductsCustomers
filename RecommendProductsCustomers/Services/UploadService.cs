using RecommendProductsCustomers.Services.Interfaces;

namespace RecommendProductsCustomers.Services
{
    public class UploadService : IUploadService
    {
        private readonly IWebHostEnvironment _environment;

        public UploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Tạo một tên duy nhất cho hình ảnh
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

            // Lưu hình ảnh vào thư mục trên máy chủ
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images/products");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về URL gốc của hình ảnh
            var imageUrl = "/images/products/" + uniqueFileName;

            return imageUrl;
        }

    }
}
