using InMemoryCache.Data;
using InMemoryCache.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace InMemoryCache.Services
{
    public class ProductService
    {
        private readonly IMemoryCache _cache;
        private readonly ProductRepository _repository;
        private readonly ILogger<ProductService> _logger;

        // Standard cache options for our entries
        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Evict if not accessed in 5 mins
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Evict after 1 hour, regardless of activity

        public ProductService(IMemoryCache cache, ProductRepository repository, ILogger<ProductService> logger)
        {
            _cache = cache;
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Gets a single product, using the cache first.
        /// </summary>
        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            // 1. Define a unique key for this specific resource.
            var cacheKey = $"product-{productId}";

            // 2. Try to get the item from the cache.
            // The 'TryGetValue' pattern is efficient and safe.
            if (_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct))
            {
                _logger.LogInformation("Cache HIT for key: {CacheKey}", cacheKey);
                return cachedProduct;
            }

            // 3. If it's a cache miss, fetch from the repository.
            _logger.LogWarning("Cache MISS for key: {CacheKey}. Fetching from repository.", cacheKey);
            var productFromRepo = await _repository.GetProductByIdAsync(productId);

            // 4. If data was found, add it to the cache before returning.
            if (productFromRepo != null)
            {
                _cache.Set(cacheKey, productFromRepo, _cacheOptions);
            }

            return productFromRepo;
        }

        /// <summary>
        /// Gets a list of products based on filters, using the cache first.
        /// </summary>
        public async Task<IEnumerable<ProductDto>> GetProductsByFilterAsync(int? categoryId, decimal? maxPrice)
        {
            // 1. Generate a dynamic and unique cache key based on ALL parameters.
            // This is crucial. A different filter combination MUST result in a different key.
            var cacheKey = GenerateFilterCacheKey(categoryId, maxPrice);

            // 2. Try to get the list from the cache.
            if (_cache.TryGetValue(cacheKey, out IEnumerable<ProductDto>? cachedProducts))
            {
                _logger.LogInformation("Cache HIT for key: {CacheKey}", cacheKey);
                return cachedProducts ?? Enumerable.Empty<ProductDto>();
            }

            // 3. If cache miss, fetch from the repository.
            _logger.LogWarning("Cache MISS for key: {CacheKey}. Fetching from repository.", cacheKey);
            var productsFromRepo = await _repository.GetProductsByFilterAsync(categoryId, maxPrice);

            // 4. Add the result to the cache.
            _cache.Set(cacheKey, productsFromRepo, _cacheOptions);

            return productsFromRepo;
        }

        /// <summary>
        /// Removes a specific product from the cache.
        /// </summary>
        public void InvalidateProductCache(int productId)
        {
            var cacheKey = $"product-{productId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Invalidated cache for key: {CacheKey}", cacheKey);

            // NOTE: In a real-world scenario, updating a single product might also require
            // invalidating list caches that contain it. This can be complex.
            // Strategies include using a CancellationChangeToken or simply clearing all
            // related list caches, though the latter can reduce cache effectiveness.
            // For this example, we only invalidate the single item cache.
        }

        /// <summary>
        /// Helper method to create a consistent cache key from filter parameters.
        /// </summary>
        private string GenerateFilterCacheKey(int? categoryId, decimal? maxPrice)
        {
            var keyBuilder = new StringBuilder("products-filter");
            keyBuilder.Append($"-cat:{categoryId?.ToString() ?? "any"}");
            keyBuilder.Append($"-price:{maxPrice?.ToString() ?? "any"}");
            return keyBuilder.ToString();
        }
    }
}
