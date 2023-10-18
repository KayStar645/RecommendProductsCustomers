namespace RecommendProductsCustomers.Models
{
	public class CustomerModel
	{
        public string? id { get; set; }
		public string? phone { get; set; }
        public string? password { get; set; }
        public string? name { get; set; }
        public string? gender { get; set; }
        public DateTime? dateBirth { get; set; }
		public string? address { get; set; }
		public List<string>? hobbies { get; set; }
		public string? rePassword { get; set; }


    }
}
