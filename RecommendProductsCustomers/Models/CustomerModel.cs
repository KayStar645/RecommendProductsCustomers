namespace RecommendProductsCustomers.Models
{
	public class CustomerModel
	{
        public string? id { get; set; }
        public string? internalCode { get; set; }
		public string? phone { get; set; }
		public string? name { get; set; }
		public DateTime? dateBirth { get; set; }
		public string? address { get; set; }

	}
}
