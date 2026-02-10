using Microsoft.EntityFrameworkCore;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;
using PosSystem.Infrastructure.Persistence;

namespace PosSystem.Infrastructure.Persistence.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _dbContext;

    public SaleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Sale>> GetAllAsync()
    {
        return await _dbContext.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .ToListAsync();
    }

    public async Task<Sale> GetByIdAsync(Guid id)
    {
        return await _dbContext.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sale> CreateAsync(Sale entity)
    {
        await _dbContext.Sales.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Sale entity)
    {
        _dbContext.Sales.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var sale = await _dbContext.Sales.FindAsync(id);
        if (sale != null)
        {
            _dbContext.Sales.Remove(sale);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task BeginTransactionAsync()
    {
        await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _dbContext.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _dbContext.Database.RollbackTransactionAsync();
    }
}