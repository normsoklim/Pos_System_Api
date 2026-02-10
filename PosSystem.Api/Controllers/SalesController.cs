using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.DTOs.Sale;
using PosSystem.Application.Interfaces.Services;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSales()
        {
            var sales = await _saleService.GetAllSalesAsync();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSale(Guid id)
        {
            var sale = await _saleService.GetSaleByIdAsync(id);
            if (sale == null)
                return NotFound();

            return Ok(sale);
        }

        [HttpPost]
        [Authorize(Policy = "CanSell")]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto createSaleDto)
        {
            var sale = await _saleService.CreateSaleAsync(createSaleDto);
            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "CanSell")]
        public async Task<IActionResult> UpdateSaleStatus(Guid id, [FromBody] string status)
        {
            await _saleService.UpdateSaleStatusAsync(id, status);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanSell")]
        public async Task<IActionResult> DeleteSale(Guid id)
        {
            await _saleService.DeleteSaleAsync(id);
            return NoContent();
        }
    }
}