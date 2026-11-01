namespace PosSystem.Application.DTOs.Payment
{
    public class BakongPaymentResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? QrCodeData { get; set; } // Base64 encoded QR code image
        public string? QrCodeText { get; set; } // Text content for QR code
        public string? PaymentUrl { get; set; }
        public string? TransactionId { get; set; }
    }
}