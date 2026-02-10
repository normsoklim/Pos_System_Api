using Microsoft.AspNetCore.Mvc;
using PosSystem.Infrastructure.Services;
using PosSystem.Application.DTOs.ProductStock;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductStockController : ControllerBase
    {
        private readonly ProductStockService _productStockService;
        private readonly ILogger<ProductStockController> _logger;

        public ProductStockController(ProductStockService productStockService, ILogger<ProductStockController> logger)
        {
            _productStockService = productStockService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all products with their corresponding stock information
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ProductStockDto>>> GetAllProductsWithStock()
        {
            try
            {
                var productStocks = await _productStockService.GetAllProductsWithStockAsync();
                return Ok(productStocks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products with stock information");
                return StatusCode(500, "An error occurred while retrieving products with stock information");
            }
        }

        /// <summary>
        /// Gets a specific product with its stock information
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductStockDto>> GetProductWithStock(Guid id)
        {
            try
            {
                var productStock = await _productStockService.GetProductWithStockAsync(id);
                if (productStock == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }
                return Ok(productStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with stock information for ID {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving product with stock information");
            }
        }

        /// <summary>
        /// Synchronizes the QtyInStock in Product table with the Quantity in Stock table
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SynchronizeProductAndStockQuantities()
        {
            try
            {
                await _productStockService.SynchronizeProductAndStockQuantitiesAsync();
                return Ok("Product and stock quantities synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing product and stock quantities");
                return StatusCode(500, "An error occurred while synchronizing product and stock quantities");
            }
        }

        /// <summary>
        /// Updates stock and automatically synchronizes with Product table
        /// </summary>
        [HttpPut("update-stock/{id:guid}")]
        public async Task<IActionResult> UpdateStockAndSyncProduct(
            Guid id, 
            [FromBody] UpdateStockAndSyncDto request)
        {
            try
            {
                await _productStockService.UpdateStockAndSyncProductAsync(
                    id, 
                    request.Quantity, 
                    request.CostPrice, 
                    request.SellPrice);
                return Ok("Stock updated and synchronized with product successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock and synchronizing with product for ID {StockId}", id);
                return StatusCode(500, "An error occurred while updating stock and synchronizing with product");
            }
        }
    }

    public class UpdateStockAndSyncDto
    {
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
    }
}