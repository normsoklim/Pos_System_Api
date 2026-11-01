using PosSystem.Application.DTOs.Payment;

namespace PosSystem.Application.Interfaces.Services
{
    public interface IBakongPaymentService
    {
        Task<BakongPaymentResponseDto> GenerateBakongPaymentAsync(BakongPaymentRequestDto request);
        string GenerateBakongQRCodeText(string merchantId, string merchantName, decimal amount, string currency = "KHR", string? description = null, string? referenceId = null);
    }
}