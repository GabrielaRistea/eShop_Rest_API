using Microsoft.EntityFrameworkCore;
using Proiect.Context;
using Proiect.Models;
using Proiect.Repositories.Interfaces;

namespace Proiect.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private ShopContext _context;
        public ProductRepository(ShopContext shopContext)
        {
            _context = shopContext;
        }
        public IEnumerable<Product> GetAll()
        {
            return _context.Products
                .Include(c => c.Category)
                .ToList();
        }
        public Product GetById(int id)
        {
            return _context.Products
                .Include(c => c.Category).FirstOrDefault(p => p.ProductID == id);
        }
        public void Save()
        {
            _context.SaveChanges();
        }

        public bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductID == id);
        }
        public void Update(Product product)
        {
            _context.Products.Update(product);
        }
        public void Create(Product product)
        {
            _context.Products.Add(product);
        }
        public void Delete(Product product)
        {
            _context.Products.Remove(product);
        }

        public List<Product> GetProductByCategory(int id)
        {
            return _context.Products
                .Include(p => p.Category)
                .Where(p => p.CatogoryId == id).ToList();
        }

        public IEnumerable<Product> GetByPriceAndStock(float? minPrice, float? maxPrice, bool? inStock)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            if (inStock.HasValue)
            {
                if (inStock == true)
                {
                    products = products.Where(p => p.Stock > 0);
                }
                else
                {
                    products = products.Where(p => p.Stock == 0 || p.Stock == null);
                }
            }
            

                return products.ToList();
        }
    }
}
