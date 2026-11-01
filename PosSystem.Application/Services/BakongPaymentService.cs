using System.Security.Cryptography;
using System.Text;
using QRCoder;
using PosSystem.Application.DTOs.Payment;
using PosSystem.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;

namespace PosSystem.Application.Services
{
    public class BakongPaymentService : IBakongPaymentService
    {
        private readonly ILogger<BakongPaymentService> _logger;

        public BakongPaymentService(ILogger<BakongPaymentService> logger)
        {
            _logger = logger;
        }

        public async Task<BakongPaymentResponseDto> GenerateBakongPaymentAsync(BakongPaymentRequestDto request)
        {
            try
            {
                // Handle currency conversion if needed
                decimal finalAmount = request.Amount;
                string finalCurrency = request.Currency;
                
                // If currency is USD, convert to KHR for Bakong payment
                if (request.Currency?.ToUpper() == "USD")
                {
                    finalAmount = request.Amount * 3981.80m; // Convert USD to KHR
                    finalCurrency = "KHR"; // Bakong uses KHR
                }

                // Generate the Bakong QR code text
                string qrCodeText = GenerateBakongQRCodeText(
                    request.MerchantId, 
                    request.MerchantName, 
                    finalAmount, 
                    finalCurrency, 
                    request.Description, 
                    request.ReferenceId
                );

                // Generate QR code image as base64
                string qrCodeData = GenerateQRCodeAsBase64(qrCodeText);

                return new BakongPaymentResponseDto
                {
                    Success = true,
                    Message = $"Bakong payment QR code generated successfully. {(request.Currency?.ToUpper() == "USD" ? $"Converted {request.Amount} USD to {finalAmount:F2} KHR at rate 3,981.80" : "")}",
                    QrCodeText = qrCodeText,
                    QrCodeData = qrCodeData,
                    TransactionId = request.ReferenceId ?? Guid.NewGuid().ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Bakong payment QR code");
                return new BakongPaymentResponseDto
                {
                    Success = false,
                    Message = $"Error generating Bakong payment: {ex.Message}"
                };
            }
        }

        public string GenerateBakongQRCodeText(string merchantId, string merchantName, decimal amount, string currency = "KHR", string? description = null, string? referenceId = null)
        {
            // Format amount to 2 decimal places
            string formattedAmount = amount.ToString("F2");
            
            // Create the Bakong QR code payload following professional payment QR code format
            var qrCodeBuilder = new StringBuilder();
            
            // Payload Format Indicator (00) - Always "01" for EMV
            qrCodeBuilder.Append("000201");
            
            // Point of Initiation Method (01) - "12" for static QR code (fixed amount)
            // Bakong typically uses static QR codes for better user experience
            qrCodeBuilder.Append("010212");
            
            // Merchant Account Information (26-51)
            // Bakong uses specific format for merchant account information
            var merchantInfoBuilder = new StringBuilder();
            
            // Globally Unique Identifier (00) - "kh.bakong.app" for Bakong
            merchantInfoBuilder.Append("0013kh.bakong.app");
            
            // Merchant ID (01) - Bakong account ID
            string merchantIdValue = merchantId ?? "norm_soklim@bkrt";
            merchantInfoBuilder.Append($"01{merchantIdValue.Length:D2}{merchantIdValue}");
            
            // Merchant Name (02) - Use proper business name format
            string merchantNameValue = merchantName ?? "NORM SOKLIM";
            if (merchantNameValue.Length > 0)
            {
                merchantInfoBuilder.Append($"02{merchantNameValue.Length:D2}{merchantNameValue}");
            }
            
            // Add merchant account information to main payload
            string merchantInfo = merchantInfoBuilder.ToString();
            qrCodeBuilder.Append($"26{merchantInfo.Length:D2}{merchantInfo}");
            
            // Merchant Category Code (52) - "0000" for default
            qrCodeBuilder.Append("52040000");
            
            // Transaction Currency (53) - "116" for KHR (Cambodian Riel)
            // Bakong always uses KHR currency code (116) even if original was USD
            qrCodeBuilder.Append("5303116");
            
            // Transaction Amount (54) - Always include amount for professional payment
            qrCodeBuilder.Append($"54{formattedAmount.Length:D2}{formattedAmount}");
            
            // Country Code (58) - "KH" for Cambodia
            qrCodeBuilder.Append("5802KH");
            
            // Merchant Name (59) - Business name (proper format with spaces)
            string businessName = merchantNameValue.Length > 25 ? merchantNameValue.Substring(0, 25) : merchantNameValue;
            qrCodeBuilder.Append($"59{businessName.Length:D2}{businessName}");
            
            // Merchant City (60) - "Phnom Penh"
            string city = "Phnom Penh";
            qrCodeBuilder.Append($"60{city.Length:D2}{city}");
            
            // Additional Data Field Template (62)
            var additionalDataBuilder = new StringBuilder();
            
            // Bill Number (01) - Reference ID
            if (!string.IsNullOrEmpty(referenceId))
            {
                string refId = referenceId.Length > 25 ? referenceId.Substring(0, 25) : referenceId;
                additionalDataBuilder.Append($"01{refId.Length:D2}{refId}");
            }
            
            // Purpose of Transaction (08) - Description
            if (!string.IsNullOrEmpty(description))
            {
                string desc = description.Length > 25 ? description.Substring(0, 25) : description;
                additionalDataBuilder.Append($"08{desc.Length:D2}{desc}");
            }
            
            // Add additional data if any fields were added
            if (additionalDataBuilder.Length > 0)
            {
                string additionalData = additionalDataBuilder.ToString();
                qrCodeBuilder.Append($"62{additionalData.Length:D2}{additionalData}");
            }
            
            // Calculate CRC checksum (63)
            string dataWithoutCRC = qrCodeBuilder.ToString();
            string crc = CalculateCRC16(dataWithoutCRC);
            qrCodeBuilder.Append($"6304{crc}");
            
            return qrCodeBuilder.ToString();
        }

        private string PadString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return new string(' ', maxLength);
            
            // Limit the string to maxLength and pad with spaces if needed
            if (input.Length > maxLength)
                return input.Substring(0, maxLength);
            else
                return input.PadRight(maxLength);
        }

        private string CalculateCRC16(string data)
        {
            // CRC-16/CCITT-FALSE algorithm for EMV QR codes
            ushort crc = 0xFFFF; // Initial value
            
            foreach (char c in data)
            {
                crc ^= (ushort)(c << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            
            return crc.ToString("X4").ToUpper();
        }

        private string GenerateQRCodeAsBase64(string qrCodeText)
        {
            try
            {
                // Generate QR code
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCoder.PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(20); // 20 = pixels per module
                        return Convert.ToBase64String(qrCodeImage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image");
                throw;
            }
        }
    }
}