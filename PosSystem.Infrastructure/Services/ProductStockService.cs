using Microsoft.EntityFrameworkCore;
using PosSystem.Application.DTOs.ProductStock;
using PosSystem.Application.DTOs.Stock;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;
using PosSystem.Infrastructure.Persistence;

namespace PosSystem.Infrastructure.Services;

public class ProductStockService
{
    private readonly AppDbContext _dbContext;

    public ProductStockService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets all products with their corresponding stock information
    /// </summary>
    public async Task<List<ProductStockDto>> GetAllProductsWithStockAsync()
    {
        var products = await _dbContext.Products
            .Include(p => p.Category)
            .ToListAsync();

        var stocks = await _dbContext.Stocks
            .ToListAsync();

        var productStockList = new List<ProductStockDto>();

        foreach (var product in products)
        {
            var stock = stocks.FirstOrDefault(s => s.ProductId == product.Id);
            
            productStockList.Add(new ProductStockDto
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Barcode = product.Barcode,
                Price = product.Price,
                ProductQtyInStock = product.QtyInStock, // Qty from Product table
                StockQuantity = stock?.Quantity ?? 0, // Qty from Stock table
                CostPrice = stock?.CostPrice ?? 0,
                SellPrice = stock?.SellPrice ?? 0,
                CategoryName = product.Category?.CategoryName ?? string.Empty,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                LastUpdated = stock?.LastUpdated ?? DateTime.MinValue
            });
        }

        return productStockList;
    }

    /// <summary>
    /// Gets a specific product with its stock information
    /// </summary>
    public async Task<ProductStockDto> GetProductWithStockAsync(Guid productId)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return null;

        var stock = await _dbContext.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        return new ProductStockDto
        {
            ProductId = product.Id,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            Price = product.Price,
            ProductQtyInStock = product.QtyInStock, // Qty from Product table
            StockQuantity = stock?.Quantity ?? 0, // Qty from Stock table
            CostPrice = stock?.CostPrice ?? 0,
            SellPrice = stock?.SellPrice ?? 0,
            CategoryName = product.Category?.CategoryName ?? string.Empty,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            LastUpdated = stock?.LastUpdated ?? DateTime.MinValue
        };
    }

    /// <summary>
    /// Synchronizes the QtyInStock in Product table with the Quantity in Stock table
    /// </summary>
    public async Task SynchronizeProductAndStockQuantitiesAsync()
    {
        var products = await _dbContext.Products.ToListAsync();
        var stocks = await _dbContext.Stocks.ToListAsync();

        foreach (var product in products)
        {
            var stock = stocks.FirstOrDefault(s => s.ProductId == product.Id);
            
            if (stock != null)
            {
                // Update the product's QtyInStock to match the stock quantity
                product.QtyInStock = stock.Quantity;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Updates stock and automatically synchronizes with Product table
    /// </summary>
    public async Task UpdateStockAndSyncProductAsync(Guid stockId, int quantity, decimal costPrice, decimal sellPrice)
    {
        var stock = await _dbContext.Stocks.FindAsync(stockId);
        if (stock == null)
            throw new Exception("Stock not found");

        // Update stock quantities
        stock.Quantity = quantity;
        stock.CostPrice = costPrice;
        stock.SellPrice = sellPrice;
        stock.LastUpdated = DateTime.UtcNow;

        // Find the corresponding product and update its QtyInStock
        var product = await _dbContext.Products.FindAsync(stock.ProductId);
        if (product != null)
        {
            product.QtyInStock = quantity;
        }

        await _dbContext.SaveChangesAsync();
    }
}