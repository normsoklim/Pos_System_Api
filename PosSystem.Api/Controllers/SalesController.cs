using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.Interfaces.Services;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly IProductService _productService;

        public SalesController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Policy = "CanSell")]
        public async Task<IActionResult> CreateSale()
        {
            // Implementation for creating a sale
            return Ok(new { message = "Sale created successfully" });
        }

        [HttpGet]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> GetSalesReport()
        {
            // Implementation for getting sales reports
            return Ok(new { message = "Sales report retrieved" });
        }
    }
}