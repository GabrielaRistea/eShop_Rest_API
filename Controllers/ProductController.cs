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
        private readonly ILuceneService _luceneService;
        public ProductController(IProductService productService, ILuceneService luceneService)
        {
            _productService = productService;
            _luceneService = luceneService;
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

        [HttpGet("predictive-search")]
        public IActionResult GetSuggestions([FromQuery] string term)
        {
            var suggestions = _productService.GetPredictiveSuggestions(term);
            return Ok(suggestions);
        }

        [HttpGet("{id}/similar")]
        public IActionResult GetSimilarProducts(int id)
        {
            try
            {
                var similarProducts = _productService.GetSimilarProducts(id, 4);
                return Ok(similarProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Eroare la calculul similaritatii: {ex.Message}");
            }
        }

        [HttpGet("advanced-search")]
        public IActionResult AdvancedSearch([FromQuery] string q, [FromQuery] string sort = "desc")
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new List<AdvancedSearchResultDto>());

            var results = _luceneService.Search(q, sort);

            return Ok(results);
        }

        [HttpPost("rebuild-index")]
        public IActionResult RebuildIndex()
        {
            var allProducts = _productService.GetAllProducts();
            _luceneService.BuildIndex(allProducts);
            return Ok("Indexul Lucene a fost regenerat cu succes!");
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
                PdfPath = p.PdfPath,
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
                PdfPath= productDto.PdfPath,
                CatogoryId = productDto.Category
            };
        }
    }
}
