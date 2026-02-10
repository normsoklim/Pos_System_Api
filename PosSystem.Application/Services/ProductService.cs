
using PosSystem.Application.DTOs.Product;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;
        private readonly IStockRepository _stockRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IFileService fileService, IStockRepository stockRepository)
        {
            _repo = productRepository;
            _categoryRepository = categoryRepository;
            _fileService = fileService;
            _stockRepository = stockRepository;
        }
        
        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repo.GetAllAsync();

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Barcode = p.Barcode,
                Price = p.Price,
                QtyInStock = p.QtyInStock,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId
            }).ToList();
        }

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Barcode = product.Barcode,
                Price = product.Price,
                QtyInStock = product.QtyInStock,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId
            };
        }
        public async Task CreateAsync(CreateProductDto dto)
        {
            // Validate that the CategoryId is not empty and exists before creating
            if (dto.CategoryId == Guid.Empty)
            {
                throw new Exception("CategoryId is required when creating a product.");
            }
            
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                throw new Exception($"Category with ID {dto.CategoryId} does not exist.");
            }

            var product = new Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                ProductName = dto.ProductName,
                Barcode = dto.Barcode,
                Price = dto.Price,
                QtyInStock = dto.QtyInStock,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId  // This will be non-null since we validated it above
            };

            await _repo.AddAsync(product);

            // Create a corresponding stock record
            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = product.QtyInStock,
                CostPrice = 0, // You might want to add cost price to the DTO
                SellPrice = product.Price,
                LastUpdated = DateTime.UtcNow
            };
            await _stockRepository.CreateAsync(stock);
        }

        public async Task UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            // Store the old image URL to delete it later if a new image is provided
            string? oldImageUrl = null;
            if (!string.IsNullOrEmpty(existingProduct.ImageUrl) && !string.IsNullOrEmpty(dto.ImageUrl) && existingProduct.ImageUrl != dto.ImageUrl)
            {
                oldImageUrl = existingProduct.ImageUrl;
            }

            // Only validate and update CategoryId if it's provided (not null)
            if (dto.CategoryId.HasValue && dto.CategoryId.Value != Guid.Empty)
            {
                var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                {
                    throw new Exception($"Category with ID {dto.CategoryId} does not exist.");
                }
                
                existingProduct.CategoryId = dto.CategoryId.Value;
            }
            // If dto.CategoryId is null, we keep the existing CategoryId

            // Store the original quantity to check if it changed
            var originalQty = existingProduct.QtyInStock;
            
            existingProduct.ProductName = dto.ProductName;
            existingProduct.Barcode = dto.Barcode;
            existingProduct.Price = dto.Price;
            existingProduct.QtyInStock = dto.QtyInStock;
            existingProduct.Description = dto.Description;
            existingProduct.ImageUrl = dto.ImageUrl;

            await _repo.UpdateAsync(existingProduct);

            // If quantity changed, update the corresponding stock record
            if (originalQty != dto.QtyInStock)
            {
                var stock = await _stockRepository.GetByProductIdAsync(id);
                if (stock != null)
                {
                    stock.Quantity = dto.QtyInStock;
                    stock.SellPrice = dto.Price; // Also update the sell price if it changed
                    stock.LastUpdated = DateTime.UtcNow;
                    await _stockRepository.UpdateAsync(stock);
                }
            }

            // Delete the old image file if a new image was provided
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await _fileService.DeleteFileAsync(oldImageUrl);
            }
        }

        public async Task UpdatePartialAsync(Guid id, UpdateProductPartialDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            // Store the old image URL to delete it later if a new image is provided
            string? oldImageUrl = null;
            if (!string.IsNullOrEmpty(existingProduct.ImageUrl) && dto.ImageUrl != null && existingProduct.ImageUrl != dto.ImageUrl)
            {
                oldImageUrl = existingProduct.ImageUrl;
            }

            // Store the original quantity to check if it changed
            var originalQty = existingProduct.QtyInStock;
            var originalPrice = existingProduct.Price;
            
            // Only update fields that are provided in the DTO
            if (!string.IsNullOrEmpty(dto.ProductName))
                existingProduct.ProductName = dto.ProductName;
                
            if (!string.IsNullOrEmpty(dto.Barcode))
                existingProduct.Barcode = dto.Barcode;
                
            if (dto.Price.HasValue)
            {
                existingProduct.Price = dto.Price.Value;

            }
                
            if (dto.QtyInStock.HasValue)
                existingProduct.QtyInStock = dto.QtyInStock.Value;
                
            if (!string.IsNullOrEmpty(dto.Description))
                existingProduct.Description = dto.Description;
                
            if (dto.ImageUrl != null) // Allow setting to null
                existingProduct.ImageUrl = dto.ImageUrl;
                
            if (dto.CategoryId.HasValue)
            {
                // Validate that the CategoryId exists before updating
                if (dto.CategoryId.Value != Guid.Empty)
                {
                    var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value);
                    if (category == null)
                    {
                        throw new Exception($"Category with ID {dto.CategoryId} does not exist.");
                    }
                    existingProduct.CategoryId = dto.CategoryId.Value;
                }
            }

            await _repo.UpdateAsync(existingProduct);

            // If quantity or price changed, update the corresponding stock record
            if ((dto.QtyInStock.HasValue && originalQty != dto.QtyInStock.Value) ||
                (dto.Price.HasValue && originalPrice != dto.Price.Value))
            {
                var stock = await _stockRepository.GetByProductIdAsync(id);
                if (stock != null)
                {
                    if (dto.QtyInStock.HasValue)
                        stock.Quantity = dto.QtyInStock.Value;
                    if (dto.Price.HasValue)
                        stock.SellPrice = dto.Price.Value; // Update the sell price if it changed
                    stock.LastUpdated = DateTime.UtcNow;
                    await _stockRepository.UpdateAsync(stock);
                }
            }

            // Delete the old image file if a new image was provided
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await _fileService.DeleteFileAsync(oldImageUrl);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            // Delete the corresponding stock record
            var stock = await _stockRepository.GetByProductIdAsync(id);
            if (stock != null)
            {
                await _stockRepository.DeleteAsync(stock.Id);
            }

            await _repo.DeleteAsync(id);
        }
    }
}