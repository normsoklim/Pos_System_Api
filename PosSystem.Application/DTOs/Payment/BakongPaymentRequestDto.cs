using System.ComponentModel.DataAnnotations;

namespace PosSystem.Application.DTOs.Payment
{
    public class BakongPaymentRequestDto
    {
        [Required]
        public string MerchantId { get; set; } = "norm_soklim@bkrt"; // Default merchant ID
        
        [Required]
        public string MerchantName { get; set; } = "NORM SOKLIM"; // Default merchant name (proper format with spaces)
        
        [Required]
        public decimal Amount { get; set; }
        
        public string? Description { get; set; }
        
        public string? ReferenceId { get; set; }
        
        public string Currency { get; set; } = "KHR"; // Cambodian Riel as default
    }
}