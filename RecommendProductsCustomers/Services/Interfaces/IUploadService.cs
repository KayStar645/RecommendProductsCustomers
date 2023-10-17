namespace RecommendProductsCustomers.Services.Interfaces
{
    public interface IUploadService
    {
        Task<string> UploadImage(IFormFile file);
    }
}
