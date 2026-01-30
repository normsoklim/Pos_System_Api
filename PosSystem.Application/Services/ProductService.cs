
using PosSystem.Application.DTOs.Product;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;

namespace PosSystem.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _repo = productRepository;
            _categoryRepository = categoryRepository;
        }
        
        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repo.GetAllAsync();

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Barcode = p.Barcode,
                Cost = p.Cost,
                SellPrice = p.Cost * 1.2m, // Assuming a 20% markup for SellPrice
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
                Cost = product.Cost,
                SellPrice = product.Cost * 1.2m, // Assuming a 20% markup for SellPrice
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
                Cost = dto.Cost,
                SellPrice = dto.Cost * 1.2m,
                QtyInStock = dto.QtyInStock,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId  // This will be non-null since we validated it above
            };

            await _repo.AddAsync(product);
        }

        public async Task UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

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

            existingProduct.ProductName = dto.ProductName;
            existingProduct.Barcode = dto.Barcode;
            existingProduct.Cost = dto.Cost;
            existingProduct.SellPrice = dto.Cost * 1.2m;
            existingProduct.QtyInStock = dto.QtyInStock;
            existingProduct.Description = dto.Description;
            existingProduct.ImageUrl = dto.ImageUrl;

            await _repo.UpdateAsync(existingProduct);
        }

        public async Task UpdatePartialAsync(Guid id, UpdateProductPartialDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            // Only update fields that are provided in the DTO
            if (!string.IsNullOrEmpty(dto.ProductName))
                existingProduct.ProductName = dto.ProductName;
                
            if (!string.IsNullOrEmpty(dto.Barcode))
                existingProduct.Barcode = dto.Barcode;
                
            if (dto.Cost.HasValue)
            {
                existingProduct.Cost = dto.Cost.Value;
                // Update SellPrice based on Cost (assuming 20% markup)
                existingProduct.SellPrice = dto.Cost.Value * 1.2m;
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
        }

        public async Task DeleteAsync(Guid id)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            await _repo.DeleteAsync(id);
        }
    }
}