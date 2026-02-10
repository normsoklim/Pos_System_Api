using PosSystem.Application.DTOs.Sale;

namespace PosSystem.Application.Interfaces.Services;

public interface ISaleService
{
    Task<List<SaleDto>> GetAllSalesAsync();
    Task<SaleDto> GetSaleByIdAsync(Guid id);
    Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto);
    Task UpdateSaleStatusAsync(Guid id, string status);
    Task DeleteSaleAsync(Guid id);
}