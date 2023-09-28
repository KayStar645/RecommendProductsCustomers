namespace RecommendProductsCustomers.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public readonly int Code = 404;
    }
}
