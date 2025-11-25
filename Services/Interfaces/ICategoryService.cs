using Proiect.Models;

namespace Proiect.Services.Interfaces
{
    public interface ICategoryService
    {
        List<Category> getAllCategories();
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);
        bool CategoryExists(int id);
        Category GetCategoryById(int id);
    }
}
