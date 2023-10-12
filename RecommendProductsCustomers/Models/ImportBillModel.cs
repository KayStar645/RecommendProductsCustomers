namespace RecommendProductsCustomers.Models
{
    public class ImportBillModel
    {
        public string? id { get; set; }
        public string? internalCode { get; set; }

        public DateTime? dateImport { get; set; }

        public string? distributor { get; set; }

        public string? contactPhone { get; set; }

        public long? price { get; set; }

        // False: Nháp, True: Đã nhập
        public bool? isActive { get; set; }

        // Nhân viên nào lập
        // Có những sản phẩm nào
    }
}
