

using PosSystem.Application.DTOs.Stock;
using PosSystem.Application.interfaces.Services;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _stockRepository;

    public StockService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<List<StockDto>> GetAllAsync()
    {
        var stocks = await _stockRepository.GetAllAsync();
        return stocks.Select(s => new StockDto
        {
            Id = s.Id,
            ProductId = s.ProductId,
            ProductName = s.Product?.ProductName ?? string.Empty,
            Quantity = s.Quantity,
            CostPrice = s.CostPrice,
            SellPrice = s.SellPrice
        }).ToList();
    }

    public async Task<StockDto> GetByIdAsync(Guid id)
    {
        var stock = await _stockRepository.GetByIdAsync(id);
        if (stock == null) return null;

        return new StockDto
        {
            Id = stock.Id,
            ProductId = stock.ProductId,
            ProductName = stock.Product?.ProductName ?? string.Empty,
            Quantity = stock.Quantity,
            CostPrice = stock.CostPrice,
            SellPrice = stock.SellPrice
        };
    }

    public async Task CreateAsync(CreateStockDto dto)
    {
        var stock = new Stock
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            CostPrice = dto.CostPrice,
            SellPrice = dto.SellPrice
        };

        await _stockRepository.CreateAsync(stock);
    }

    public async Task UpdateAsync(Guid id, UpdateStockDto dto)
    {
        var stock = await _stockRepository.GetByIdAsync(id);
        if (stock == null) return;

        stock.ProductId = dto.ProductId;
        stock.Quantity = dto.Quantity;
        stock.CostPrice = dto.CostPrice;
        stock.SellPrice = dto.SellPrice;

        await _stockRepository.UpdateAsync(stock);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _stockRepository.DeleteAsync(id);
    }
}