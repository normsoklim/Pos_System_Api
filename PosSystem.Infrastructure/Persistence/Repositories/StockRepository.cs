
using Microsoft.EntityFrameworkCore;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;

namespace PosSystem.Infrastructure.Persistence.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly AppDbContext _dbContext;

        public StockRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Stock>> GetAllAsync()
        {
            return await _dbContext.Stocks
                .Include(s => s.Product)
                .ToListAsync();
        }
        public async Task<Stock> GetByIdAsync(Guid id)
        {
            return await _dbContext.Stocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Stock> GetByProductIdAsync(Guid productId)
        {
            return await _dbContext.Stocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        public async Task CreateAsync(Stock entity)
        {
            await _dbContext.Stocks.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock entity)
        {
            _dbContext.Stocks.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var stock = await _dbContext.Stocks.FindAsync(id);
            if (stock != null)
            {
                _dbContext.Stocks.Remove(stock);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}

