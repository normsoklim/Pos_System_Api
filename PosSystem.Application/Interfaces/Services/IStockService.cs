
using PosSystem.Application.DTOs.Stock;

namespace PosSystem.Application.interfaces.Services;

public interface IStockService
{
    Task<List<StockDto>> GetAllAsync(); 
    Task<StockDto> GetByIdAsync(Guid id);
    Task CreateAsync(CreateStockDto dto);
    Task UpdateAsync(Guid id, UpdateStockDto dto);
    Task DeleteAsync(Guid id);
}