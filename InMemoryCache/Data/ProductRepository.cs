using InMemoryCache.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InMemoryCache.Data
{
    public class ProductRepository
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Category> _categoryCollection;

        public ProductRepository(IMongoClient mongoClient, IConfiguration configuration)
        {
            var mongoSection = configuration.GetSection("MongoDB");
            var database = mongoClient.GetDatabase(mongoSection["DatabaseName"]);
            _productCollection = database.GetCollection<Product>(mongoSection["CollectionNameProduct"]);
            _categoryCollection = database.GetCollection<Category>(mongoSection["CollectionNameCategory"]);
        }

        /// <summary>
        /// Fetches a single product and its category by product ID.
        /// </summary>
        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            var product = await _productCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();
            if (product == null)
                return null;

            var category = await _categoryCollection.Find(c => c.Id == product.CategoryId).FirstOrDefaultAsync();
            return new ProductDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                CategoryName = category?.Name ?? "Unknown"
            };
        }

        /// <summary>
        /// Fetches a list of products filtered by category ID and/or maximum price.
        /// </summary>
        public async Task<IEnumerable<ProductDto>> GetProductsByFilterAsync(int? categoryId, decimal? maxPrice)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filter = filterBuilder.Empty;

            if (categoryId.HasValue)
                filter &= filterBuilder.Eq(p => p.CategoryId, categoryId.Value);

            if (maxPrice.HasValue)
                filter &= filterBuilder.Lte(p => p.Price, maxPrice.Value);

            var products = await _productCollection.Find(filter).ToListAsync();

            var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
            var categories = await _categoryCollection.Find(c => categoryIds.Contains(c.Id)).ToListAsync();

            // LEFT JOIN: include products even if their category is missing
            var result = from p in products
                         join c in categories on p.CategoryId equals c.Id into pc
                         from c in pc.DefaultIfEmpty()
                         select new ProductDto
                         {
                             ProductId = p.Id,
                             ProductName = p.Name,
                             Price = p.Price,
                             CategoryName = c?.Name ?? "Unknown"
                         };

            return result.ToList();
        }
    }
}
