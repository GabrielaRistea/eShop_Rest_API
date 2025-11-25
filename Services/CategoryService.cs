using Proiect.Models;
using Proiect.Repositories.Interfaces;
using Proiect.Services.Interfaces;

namespace Proiect.Services
{
    public class CategoryService : ICategoryService
    {
        private ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public List<Category> getAllCategories()
        {
            return _categoryRepository.GetAll().ToList();
        }
        public void AddCategory(Category category)
        {
            _categoryRepository.Create(category);
            _categoryRepository.Save();
        }
        public void UpdateCategory(Category category)
        {
            _categoryRepository.Update(category);
            _categoryRepository.Save();
        }
        public void DeleteCategory(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category != null)
            {
                _categoryRepository.Delete(category);
                _categoryRepository.Save();
            }
        }
        public bool CategoryExists(int id)
        {
            return _categoryRepository.CategoryExists(id);
        }
        public Category GetCategoryById(int id)
        {
            return _categoryRepository.GetById(id);
        }
    }
}
