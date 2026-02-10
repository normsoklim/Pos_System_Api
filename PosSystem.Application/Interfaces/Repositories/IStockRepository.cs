

using PosSystem.Application.DTOs.Stock;
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Repositories;

public interface IStockRepository
{
    Task<List<Stock>> GetAllAsync();
    Task<Stock> GetByIdAsync(Guid id);
    Task<Stock> GetByProductIdAsync(Guid productId);
    Task CreateAsync(Stock entity);
    Task UpdateAsync(Stock entity);
    Task DeleteAsync(Guid id);
}