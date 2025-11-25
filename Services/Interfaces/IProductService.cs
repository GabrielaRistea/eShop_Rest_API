using Proiect.DTOs;
using Proiect.Models;

namespace Proiect.Services.Interfaces
{
    public interface IProductService
    {
        Product GetProductById(int id);
        Task AddProductAsync(ProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        void DeleteProduct(int id);
        List<Product> GetAllProducts();
        bool ProductExists(int id);
        List<Product> searchProduct(string name);
        Task<List<ProductDto>> ProductsByCategory(int id);
    }
}
