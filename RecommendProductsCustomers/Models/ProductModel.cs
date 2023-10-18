namespace RecommendProductsCustomers.Models
{
    public class ProductModel
    {
        public string? id { get; set; }

        public string? internalCode { get; set; }

        public string? name { get; set; }

        public string? description { get; set; }

        public string? size { get; set; }

        // Vật liệu
        public string? material { get; set; }

        // Hướng dẫn bảo quản
        public string? preserve { get; set; }

        // Hình ảnh gốc lấy cái đầu
        public List<string>? images { get; set; }

        public int? quantity { get; set; }

        public long? price { get; set; }

        public List<string>? hobbies { get; set;}
    }
}
