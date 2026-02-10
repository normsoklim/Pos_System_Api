using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Repositories;

public interface ISaleRepository
{
    Task<List<Sale>> GetAllAsync();
    Task<Sale> GetByIdAsync(Guid id);
    Task<Sale> CreateAsync(Sale entity);
    Task UpdateAsync(Sale entity);
    Task DeleteAsync(Guid id);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}