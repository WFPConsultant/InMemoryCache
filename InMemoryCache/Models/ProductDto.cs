namespace InMemoryCache.Models
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public required string CategoryName { get; set; }
    }
}
