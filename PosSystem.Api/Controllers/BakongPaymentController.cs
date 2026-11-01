using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.DTOs.Payment;
using PosSystem.Application.Interfaces.Services;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/bakong-payment")]  // Explicit route to match expected format
    public class BakongPaymentController : ControllerBase
    {
        private readonly IBakongPaymentService _bakongPaymentService;

        public BakongPaymentController(IBakongPaymentService bakongPaymentService)
        {
            _bakongPaymentService = bakongPaymentService;
        }

        [HttpPost("generate")]
        [Authorize]
        public async Task<IActionResult> GenerateBakongPayment([FromBody] BakongPaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _bakongPaymentService.GenerateBakongPaymentAsync(request);
            
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        [HttpGet("qr-code")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBakongQRCode(
            [FromQuery] string merchantId = "norm_soklim@bkrt",
            [FromQuery] string merchantName = "NORM SOKLIM",
            [FromQuery] decimal amount = 0,
            [FromQuery] string currency = "KHR",
            [FromQuery] string? description = null,
            [FromQuery] string? referenceId = null)
        {
            var request = new BakongPaymentRequestDto
            {
                MerchantId = merchantId,
                MerchantName = merchantName,
                Amount = amount,
                Currency = currency,
                Description = description,
                ReferenceId = referenceId
            };

            var response = await _bakongPaymentService.GenerateBakongPaymentAsync(request);
            
            if (!response.Success || string.IsNullOrEmpty(response.QrCodeData))
            {
                return BadRequest(new { message = response?.Message ?? "Failed to generate QR code" });
            }

            // Return the QR code image
            byte[] imageBytes = Convert.FromBase64String(response.QrCodeData);
            return File(imageBytes, "image/png");
        }
    }
}