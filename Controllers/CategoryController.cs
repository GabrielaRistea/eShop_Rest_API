using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Proiect.Models;
using Proiect.Services.Interfaces;

namespace Proiect.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/<CategoryController>
        [HttpGet]
        public IActionResult GetAll()
        {
            var category = _categoryService.getAllCategories();
            return Ok(category);
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public IActionResult GetById(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = _categoryService.GetCategoryById(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST api/<CategoryController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] Category category)
        {
            var categories = _categoryService.getAllCategories();

            _categoryService.AddCategory(category);
            return Ok(category);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, [FromBody] Category category)
        {
            if (id != category.CategoryID)
            {
                return BadRequest();
            }

            _categoryService.UpdateCategory(category);

            return NoContent();

        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category != null)
            {
                _categoryService.DeleteCategory(id);
            }
            return NoContent();
        }
    }
}
