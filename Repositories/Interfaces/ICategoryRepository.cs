using Proiect.Models;

namespace Proiect.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        bool CategoryExists(int id);
        void Create(Category category);
        void Update(Category category);
        void Delete(Category category);
        void Save();
        Category GetById(int id);
    }
}
