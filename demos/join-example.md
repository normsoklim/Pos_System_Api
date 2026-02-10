# How to Join Product and Stock Tables

This document explains how to join the Product and Stock tables to get quantity information from both sources.

## Method 1: Using Entity Framework with Include

```csharp
// In your service or repository
public async Task<List<ProductStockDto>> GetProductsWithStockAsync()
{
    var productsWithStock = await _dbContext.Products
        .Include(p => p.Category) // Include related category data
        .Select(p => new ProductStockDto
        {
            ProductId = p.Id,
            ProductName = p.ProductName,
            ProductQtyInStock = p.QtyInStock, // Qty from Product table
            Barcode = p.Barcode,
            Price = p.Price,
            CategoryName = p.Category.CategoryName,
            // Join with Stock table to get stock-specific data
            StockQuantity = _dbContext.Stocks
                .Where(s => s.ProductId == p.Id)
                .Select(s => s.Quantity)
                .FirstOrDefault(), // Qty from Stock table
            CostPrice = _dbContext.Stocks
                .Where(s => s.ProductId == p.Id)
                .Select(s => s.CostPrice)
                .FirstOrDefault(),
            SellPrice = _dbContext.Stocks
                .Where(s => s.ProductId == p.Id)
                .Select(s => s.SellPrice)
                .FirstOrDefault(),
            LastUpdated = _dbContext.Stocks
                .Where(s => s.ProductId == p.Id)
                .Select(s => s.LastUpdated)
                .FirstOrDefault()
        })
        .ToListAsync();

    return productsWithStock;
}
```

## Method 2: Using Join Query

```csharp
public async Task<List<ProductStockDto>> GetProductsWithStockJoinAsync()
{
    var query = from product in _dbContext.Products
                join stock in _dbContext.Stocks
                on product.Id equals stock.ProductId into stockGroup
                from stock in stockGroup.DefaultIfEmpty() // Left join to include products without stock
                select new ProductStockDto
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    ProductQtyInStock = product.QtyInStock, // Qty from Product table
                    StockQuantity = stock != null ? stock.Quantity : 0, // Qty from Stock table
                    Barcode = product.Barcode,
                    Price = product.Price,
                    CategoryName = product.Category.CategoryName,
                    CostPrice = stock != null ? stock.CostPrice : 0,
                    SellPrice = stock != null ? stock.SellPrice : 0,
                    LastUpdated = stock != null ? stock.LastUpdated : DateTime.MinValue
                };

    return await query.ToListAsync();
}
```

## Method 3: Two-Step Approach (as implemented in ProductStockService)

```csharp
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
            CategoryName = product.Category?.CategoryName,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            LastUpdated = stock?.LastUpdated ?? DateTime.MinValue
        });
    }

    return productStockList;
}
```

## Key Points:

1. **Product.QtyInStock**: This is the quantity field in the Product table
2. **Stock.Quantity**: This is the quantity field in the Stock table
3. The Stock table provides more detailed information (CostPrice, SellPrice, LastUpdated) in addition to quantity
4. The relationship is one-to-one per product (one product has one stock record)

## API Endpoints Available:

- `GET /api/productstock` - Get all products with stock information
- `GET /api/productstock/{id}` - Get specific product with stock information
- `POST /api/productstock/sync` - Synchronize quantities between Product and Stock tables
- `PUT /api/productstock/update-stock/{id}` - Update stock and sync with Product table

## DTO Structure:

```csharp
public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string Barcode { get; set; }
    public decimal Price { get; set; }
    public int ProductQtyInStock { get; set; } // Qty from Product table
    public int StockQuantity { get; set; }     // Qty from Stock table
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

This approach allows you to see both quantities side-by-side and keep them synchronized as needed.