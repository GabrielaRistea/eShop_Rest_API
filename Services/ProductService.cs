using Proiect.DTOs;
using Proiect.Models;
using Proiect.Repositories.Interfaces;
using Proiect.Services.Interfaces;

namespace Proiect.Services
{
    public class ProductService : IProductService
    {
        private IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public Product GetProductById(int id)
        {
            return _productRepository.GetById(id);
        }
        public async Task AddProductAsync(ProductDto productDto)
        {
            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await productDto.ImageFile.CopyToAsync(ms);
                    productDto.ProductImage = ms.ToArray();
                }
            }

            var newProduct = new Product()
            {
                ProductID = 0,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageFile = productDto.ImageFile,
                ProductImage = productDto.ProductImage,
                CatogoryId = productDto.Category,
            };

            _productRepository.Create(newProduct);
            _productRepository.Save();
        }
        public async Task UpdateProductAsync(ProductDto productDto)
        {
            using var ms = new MemoryStream();

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                await productDto.ImageFile.CopyToAsync(ms);
                productDto.ProductImage = ms.ToArray();
            }
            var newProduct = new Product()
            {
                ProductID = productDto.Id,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageFile = productDto.ImageFile,
                ProductImage = productDto.ProductImage,
                CatogoryId = productDto.Category,
            };
            _productRepository.Update(newProduct);
            _productRepository.Save();
        }
        public void DeleteProduct(int id)
        {
            var product = _productRepository.GetById(id);
            if (product != null)
            {
                _productRepository.Delete(product);
                _productRepository.Save();
            }
        }
        public bool ProductExists(int id)
        {
            return _productRepository.ProductExists(id);
        }
        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAll().ToList();
        }
        public List<Product> searchProduct(string name)
        {
            var product = _productRepository.GetAll();

            product = product.Where(p => p.Name != null ? p.Name.Contains(name) : true).ToList();

            return product.ToList();
        }

        public async Task<List<ProductDto>> ProductsByCategory(int id)
        {
            var products = _productRepository.GetProductByCategory(id);
            return products.Select(p => new ProductDto
            {
                Id = p.ProductID,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ProductImage = p.ProductImage,
                ImageFile = p.ImageFile,
                Category = p.CatogoryId,
                CategoryName = p.Category?.Name ?? "Uncategorized",
            }).ToList();
        }
    }
}
