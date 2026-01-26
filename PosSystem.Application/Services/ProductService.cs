
using PosSystem.Application.DTOs.Product;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;

namespace PosSystem.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository productRepository)
        {
            _repo = productRepository;
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
                CategoryId = dto.CategoryId
            };

            await _repo.AddAsync(product);
        }

        public async Task UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Product not found");

            existingProduct.ProductName = dto.ProductName;
            existingProduct.Barcode = dto.Barcode;
            existingProduct.Cost = dto.Cost;
            existingProduct.SellPrice = dto.Cost * 1.2m;
            existingProduct.QtyInStock = dto.QtyInStock;
            existingProduct.Description = dto.Description;
            existingProduct.ImageUrl = dto.ImageUrl;
            existingProduct.CategoryId = dto.CategoryId;

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