using InMemoryCache.Services;
using Microsoft.AspNetCore.Mvc;

namespace InMemoryCache.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Gets a single product by its ID.
        /// </summary>
        /// <remarks>
        /// Try calling this multiple times. The first call will be slower (cache miss),
        /// subsequent calls within 5 minutes will be very fast (cache hit).
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return product != null ? Ok(product) : NotFound();
        }

        /// <summary>
        /// Searches for products using optional filters.
        /// </summary>
        /// <remarks>
        /// Example URLs:
        /// /api/products/search
        /// /api/products/search?categoryId=2
        /// /api/products/search?categoryId=1&maxPrice=100
        /// Each unique combination of parameters will have its own cache entry.
        /// </remarks>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] int? categoryId, [FromQuery] decimal? maxPrice)
        {
            var products = await _productService.GetProductsByFilterAsync(categoryId, maxPrice);
            return Ok(products);
        }

        /// <summary>
        /// Invalidates the cache for a single product.
        /// </summary>
        /// <remarks>
        /// Call this endpoint after you update or delete a product in the database.
        /// The next time GET /api/products/{id} is called, it will fetch fresh data.
        /// </remarks>
        [HttpDelete("{id}/cache")]
        public IActionResult InvalidateCache(int id)
        {
            _productService.InvalidateProductCache(id);
            return NoContent(); // 204 No Content is a standard response for a successful delete/invalidation.
        }
    }
}
