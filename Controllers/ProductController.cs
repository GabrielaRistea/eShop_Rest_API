using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proiect.DTOs;
using Proiect.Models;
using Proiect.Services;
using Proiect.Services.Interfaces;

namespace Proiect.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        //[ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var product = _productService.GetAllProducts().Select(p => mapProduct(p)).ToList();

            return Ok(product);
        }
        [HttpGet("by-product-name/{name}")]
        //[ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public IActionResult SearchProduct(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return NotFound();
            }
            var product = _productService.searchProduct(name).Select(p => mapProduct(p)).ToList();
            return Ok(product);
        }
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = _productService.GetProductById(id);

            if (product == null)
                return NotFound();

            return Ok(mapProduct(product));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductDto productDto)
        {
            var products = mapProduct(productDto);

            await _productService.AddProductAsync(productDto);
            //return CreatedAtAction(nameof(GetById), new { id = artist.ArtistID }, artist);
            return Ok(productDto);

        }
        [HttpGet("by-category-id/{categoryId}")]
        //[ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            if (categoryId == null)
            {
                return NotFound();
            }
            var products = await _productService.ProductsByCategory(categoryId);
            return Ok(products);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditAsync(int id, [FromForm] ProductDto productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest();
            }
            var product = mapProduct(productDto);
            await _productService.UpdateProductAsync(productDto);

            return NoContent();

        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _productService.GetProductById(id);
            if (product != null)
            {
                _productService.DeleteProduct(id);
            }
            return NoContent();
        }

        [HttpGet("filter")]
        public IActionResult FilterProducts([FromQuery] float? minPrice, [FromQuery] float? maxPrice, [FromQuery] bool? inStock, [FromQuery] int? categoryId)
        {
            try
            {
                var products = _productService.GetProductsByFilters(minPrice, maxPrice, inStock, categoryId);

                var productDtos = products.Select(p => mapProduct(p)).ToList();

                return Ok(productDtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private ProductDto mapProduct(Product p)
        {
            return new ProductDto()
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
            };
        }

        private Product mapProduct(ProductDto productDto)
        {
            return new Product()
            {
                ProductID = productDto.Id,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageFile = productDto.ImageFile,
                ProductImage = productDto.ProductImage,
                CatogoryId = productDto.Category
            };
        }
    }
}
