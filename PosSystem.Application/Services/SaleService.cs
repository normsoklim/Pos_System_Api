using PosSystem.Application.DTOs.Sale;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Domain.Entities;
using PosSystem.Application.DTOs.Payment;

namespace PosSystem.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IBakongPaymentService _bakongPaymentService;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IStockRepository stockRepository,
        IBakongPaymentService bakongPaymentService)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _stockRepository = stockRepository;
        _bakongPaymentService = bakongPaymentService;
    }

    public async Task<List<SaleDto>> GetAllSalesAsync()
    {
        var sales = await _saleRepository.GetAllAsync();
        return sales.Select(s => new SaleDto
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            CustomerName = s.Customer?.CustomerName ?? string.Empty,
            TotalAmount = s.TotalAmount,
            Discount = s.Discount,
            Tax = s.Tax,
            FinalAmount = s.FinalAmount,
            PaymentMethod = s.PaymentMethod,
            Status = s.Status,
            SaleDate = s.SaleDate,
            Notes = s.Notes,
            SaleItems = s.SaleItems.Select(si => new SaleItemDto
            {
                ProductId = si.ProductId,
                ProductName = si.Product?.ProductName,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.TotalPrice
            }).ToList()
        }).ToList();
    }

    public async Task<SaleDto> GetSaleByIdAsync(Guid id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null)
            return null;

        return new SaleDto
        {
            Id = sale.Id,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.CustomerName ?? string.Empty,
            TotalAmount = sale.TotalAmount,
            Discount = sale.Discount,
            Tax = sale.Tax,
            FinalAmount = sale.FinalAmount,
            PaymentMethod = sale.PaymentMethod,
            Status = sale.Status,
            SaleDate = sale.SaleDate,
            Notes = sale.Notes,
            SaleItems = sale.SaleItems.Select(si => new SaleItemDto
            {
                ProductId = si.ProductId,
                ProductName = si.Product?.ProductName,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.TotalPrice
            }).ToList()
        };
    }
public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto)
{
    // Process each sale item to calculate total amount with automatic price fetching
    decimal totalAmount = 0;
    foreach (var item in createSaleDto.SaleItems)
    {
        // Get the product to verify it exists and fetch price if not provided
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        if (product == null)
            throw new ArgumentException($"Product with ID {item.ProductId} does not exist");

        // Use provided unit price or fetch from product
        decimal unitPrice = item.UnitPrice ?? product.Price;
        
        // Add to total amount
        totalAmount += item.Quantity * unitPrice;
    }

    // Create the sale
    var sale = new Sale
    {
        CustomerId = createSaleDto.CustomerId,
        TotalAmount = totalAmount,
        Discount = createSaleDto.Discount,
        Tax = createSaleDto.Tax,
        FinalAmount = totalAmount + createSaleDto.Tax - createSaleDto.Discount,
        PaymentMethod = createSaleDto.PaymentMethod ?? "Cash", // Default to Cash if not specified
        Notes = createSaleDto.Notes,
        SaleDate = DateTime.UtcNow
    };

    // Process each sale item
    foreach (var item in createSaleDto.SaleItems)
    {
        // Get the product to verify it exists and fetch price if not provided
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        if (product == null)
            throw new ArgumentException($"Product with ID {item.ProductId} does not exist");

        // Get the current stock for this product
        var currentStock = await _stockRepository.GetByProductIdAsync(item.ProductId);
        if (currentStock == null)
        {
            // Create a new stock record if one doesn't exist
            // Initialize with the product's QtyInStock value, but this should ideally be handled when product is created
            currentStock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = product.QtyInStock >= item.Quantity ? product.QtyInStock : item.Quantity, // Use the larger value to avoid negative stock
                CostPrice = 0, // Default cost price - you might want to get this from product
                SellPrice = product.Price, // Use product price as sell price
                LastUpdated = DateTime.UtcNow
            };
            await _stockRepository.CreateAsync(currentStock);
            
            // Update the product's QtyInStock to match the stock record for consistency
            product.QtyInStock = currentStock.Quantity;
            await _productRepository.UpdateAsync(product);
        }

        // Check if there's enough stock
        if (currentStock.Quantity < item.Quantity)
            throw new InvalidOperationException($"Insufficient stock for product {product.ProductName}. Available: {currentStock.Quantity}, Requested: {item.Quantity}");

        // Use provided unit price or fetch from product
        decimal unitPrice = item.UnitPrice ?? product.Price;

        // Create the sale item
        var saleItem = new SaleItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = unitPrice,
            TotalPrice = item.Quantity * unitPrice
        };
        
        sale.SaleItems.Add(saleItem);

        // Update the stock quantity
        currentStock.Quantity -= item.Quantity;
        await _stockRepository.UpdateAsync(currentStock);
        
        // Also update the product's QtyInStock to keep it in sync
        product.QtyInStock -= item.Quantity;
        await _productRepository.UpdateAsync(product);
    }

    // Save the sale
    var createdSale = await _saleRepository.CreateAsync(sale);
    
    return new SaleDto
    {
        Id = createdSale.Id,
        CustomerId = createdSale.CustomerId,
        CustomerName = createdSale.Customer?.CustomerName ?? string.Empty,
        TotalAmount = createdSale.TotalAmount,
        Discount = createdSale.Discount,
        Tax = createdSale.Tax,
        FinalAmount = createdSale.FinalAmount,
        PaymentMethod = createdSale.PaymentMethod,
        Status = createdSale.Status,
        SaleDate = createdSale.SaleDate,
        Notes = createdSale.Notes,
        SaleItems = createdSale.SaleItems.Select(si => new SaleItemDto
        {
            ProductId = si.ProductId,
            ProductName = si.Product?.ProductName,
            Quantity = si.Quantity,
            UnitPrice = si.UnitPrice,
            TotalPrice = si.TotalPrice
        }).ToList()
    };
}


    public async Task UpdateSaleStatusAsync(Guid id, string status)
    {
        var existingSale = await _saleRepository.GetByIdAsync(id);
        if (existingSale == null)
            throw new ArgumentException($"Sale with ID {id} does not exist");

        existingSale.Status = status;
        await _saleRepository.UpdateAsync(existingSale);
    }

    public async Task DeleteSaleAsync(Guid id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null)
            return;

        // In a real system, you might want to restore stock quantities when deleting a sale
        // For now, we'll just delete the sale
        await _saleRepository.DeleteAsync(id);
    }

    public async Task<BakongPaymentResponseDto> ProcessBakongPaymentForSaleAsync(Guid saleId, string? description = null)
    {
        // Get the sale
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null)
            throw new ArgumentException($"Sale with ID {saleId} does not exist");

        // Create Bakong payment request
        var paymentRequest = new BakongPaymentRequestDto
        {
            MerchantId = "norm_soklim@bkrt", // Using the provided merchant ID
            MerchantName = "NORM SOKLIM",    // Using the provided merchant name format
            Amount = sale.TotalAmount,
            Currency = "KHR",
            Description = description ?? $"Payment for sale #{sale.Id}",
            ReferenceId = sale.Id.ToString()
        };

        // Generate Bakong payment
        var paymentResponse = await _bakongPaymentService.GenerateBakongPaymentAsync(paymentRequest);

        // Update the sale's payment method to Bakong KH QR
        sale.PaymentMethod = "Bakong KH QR";
        await _saleRepository.UpdateAsync(sale);

        return paymentResponse;
    }
}