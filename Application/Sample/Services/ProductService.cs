using Application.Sample.DTOs;
using Application.Sample.Interfaces;
using Domain.Sample.Entities;

namespace Application.Sample.Services
{
    public class ProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _repo.GetAllAsync();
            return products.Select(p => new ProductDto(p.Id, p.Name, p.Price));
        }

        public async Task<ProductDto> AddAsync(string name, decimal price)
        {
            var product = new Product { Name = name, Price = price };
            var created = await _repo.AddAsync(product);
            return new ProductDto(created.Id, created.Name, created.Price);
        }
    }
}
