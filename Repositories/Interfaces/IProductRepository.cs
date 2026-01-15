using Proiect.Models;

namespace Proiect.Repositories.Interfaces
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAll();
        Product GetById(int id);
        bool ProductExists(int id);
        void Create(Product product);
        void Update(Product product);
        void Delete(Product product);
        void Save();
        List<Product> GetProductByCategory(int id);
        IEnumerable<Product> GetProductsByFilters(float? minPrice, float? maxPrice, bool? inStock, int? categoryId);
    }
}
