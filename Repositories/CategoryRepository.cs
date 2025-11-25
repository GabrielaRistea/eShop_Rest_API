using Proiect.Context;
using Proiect.Models;
using Proiect.Repositories.Interfaces;

namespace Proiect.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private ShopContext _context;
        public CategoryRepository(ShopContext shopContext)
        {
            _context = shopContext;
        }
        public IEnumerable<Category> GetAll()
        {
            return _context.Categories.ToList();
        }
        public void Create(Category category)
        {
            _context.Categories.Add(category);
        }
        public void Delete(Category category)
        {
            _context.Categories.Remove(category);
        }
        public void Save()
        {
            _context.SaveChanges();
        }

        public bool CategoryExists(int id)
        {
            return _context.Categories.Any(g => g.CategoryID == id);
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
        public Category GetById(int id)
        {
            return _context.Categories.FirstOrDefault(g => g.CategoryID == id);
        }
    }
}
