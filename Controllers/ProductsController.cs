using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureMongoApi.Filters;
using SecureMongoApi.Models;
using SecureMongoApi.Repositories;

namespace SecureMongoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("Fixed")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet]
        //[AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { Message = "Product not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id: {Id}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateModel]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            try
            {
                var createdProduct = await _productRepository.CreateAsync(product);
                _logger.LogInformation("Product created: {ProductId}", createdProduct.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateModel]
        public async Task<IActionResult> Update(string id, [FromBody] Product product)
        {
            try
            {
                var updatedProduct = await _productRepository.UpdateAsync(id, product);
                _logger.LogInformation("Product updated: {ProductId}", id);
                return Ok(updatedProduct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Product not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _productRepository.DeleteAsync(id);
                if (!result)
                    return NotFound(new { Message = "Product not found" });

                _logger.LogInformation("Product deleted: {ProductId}", id);
                return Ok(new { Message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(category);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {Category}", category);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}