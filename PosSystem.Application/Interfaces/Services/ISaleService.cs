using PosSystem.Application.DTOs.Sale;
using PosSystem.Application.DTOs.Payment;

namespace PosSystem.Application.Interfaces.Services;

public interface ISaleService
{
    Task<List<SaleDto>> GetAllSalesAsync();
    Task<SaleDto> GetSaleByIdAsync(Guid id);
    Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto);
    Task UpdateSaleStatusAsync(Guid id, string status);
    Task DeleteSaleAsync(Guid id);
    Task<BakongPaymentResponseDto> ProcessBakongPaymentForSaleAsync(Guid saleId, string? description = null);
}