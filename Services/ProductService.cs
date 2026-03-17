using NuGet.Versioning;
using Proiect.DTOs;
using Proiect.Models;
using Proiect.Repositories.Interfaces;
using Proiect.Services.Interfaces;

namespace Proiect.Services
{
    public class ProductService : IProductService
    {
        private IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public Product GetProductById(int id)
        {
            return _productRepository.GetById(id);
        }
        public async Task AddProductAsync(ProductDto productDto)
        {
            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await productDto.ImageFile.CopyToAsync(ms);
                    productDto.ProductImage = ms.ToArray();
                }
            }

            if (productDto.PdfFile != null && productDto.PdfFile.Length > 0)
            {
                var fileName = productDto.PdfFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/documents", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.PdfFile.CopyToAsync(stream);
                }
                productDto.PdfPath = "/documents/" + fileName; 
            }

            var newProduct = new Product()
            {
                ProductID = 0,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageFile = productDto.ImageFile,
                ProductImage = productDto.ProductImage,
                PdfPath = productDto.PdfPath,
                CatogoryId = productDto.Category,
            };

            _productRepository.Create(newProduct);
            _productRepository.Save();
        }
        public async Task UpdateProductAsync(ProductDto productDto)
        {
           

            var existingProduct = _productRepository.GetById(productDto.Id);

            if (existingProduct == null) return;

            if (productDto.PdfFile != null && productDto.PdfFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProduct.PdfPath))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.PdfPath.TrimStart('/'));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                var fileName = productDto.PdfFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/documents", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.PdfFile.CopyToAsync(stream);
                }
                existingProduct.PdfPath = "/documents/" + fileName;
            }
            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;
            existingProduct.CatogoryId = productDto.Category;
            using var ms = new MemoryStream();

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                await productDto.ImageFile.CopyToAsync(ms);
                productDto.ProductImage = ms.ToArray();
            }

            //var newProduct = new Product()
            //{
            //    ProductID = productDto.Id,
            //    Name = productDto.Name,
            //    Description = productDto.Description,
            //    Price = productDto.Price,
            //    Stock = productDto.Stock,
            //    ImageFile = productDto.ImageFile,
            //    ProductImage = productDto.ProductImage,
            //    PdfPath = productDto.PdfPath,
            //    CatogoryId = productDto.Category,
            //};
            _productRepository.Update(existingProduct);
            _productRepository.Save();
        }
        public void DeleteProduct(int id)
        {
            var product = _productRepository.GetById(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.PdfPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.PdfPath.TrimStart('/'));
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
                _productRepository.Delete(product);
                _productRepository.Save();
            }
        }
        public bool ProductExists(int id)
        {
            return _productRepository.ProductExists(id);
        }
        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAll().ToList();
        }
        //public List<Product> searchProduct(string name)
        //{
        //    var product = _productRepository.GetAll();

        //    product = product.Where(p => p.Name != null ? p.Name.StartsWith(name) : true).ToList();

        //    return product.ToList();
        //}

        public List<Product> searchProduct(string query)
        {
            var allProducts = _productRepository.GetAll().ToList();

            if (string.IsNullOrWhiteSpace(query))
                return allProducts;

            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var scoredResults = new List<(Product Product, double Score)>();

            var idfWeights = new Dictionary<string, double>();
            foreach (var term in searchTerms)
            {
                int titlesWithTerm = allProducts.Count(p =>
                    p.Name != null && p.Name.ToLower().Contains(term));

                double idf = Math.Log((double)allProducts.Count / (titlesWithTerm > 0 ? titlesWithTerm : 1) + 1.0);
                idfWeights[term] = idf;
            }

            foreach (var product in allProducts)
            {
                double productScore = 0;
                if (string.IsNullOrEmpty(product.Name)) continue;

                var titleWords = product.Name.ToLower()
                    .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var term in searchTerms)
                {
                    int countInTitle = titleWords.Count(w => w == term);

                    double tf = (double)countInTitle / titleWords.Length;

                    if (product.Name.ToLower().StartsWith(term))
                    {
                        productScore += 0.1; 
                    }

                    productScore += tf * idfWeights[term];
                }

                if (productScore > 0)
                {
                    scoredResults.Add((product, productScore));
                }
            }

            return scoredResults
                .OrderByDescending(r => r.Score)
                .Select(r => r.Product)
                .ToList();
        }

        public async Task<List<ProductDto>> ProductsByCategory(int id)
        {
            var products = _productRepository.GetProductByCategory(id);
            return products.Select(p => new ProductDto
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
            }).ToList();
        }

        public List<Product> GetProductsByFilters(float? minPrice, float? maxPrice, bool? inStock, int? categoryId)
        {
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                throw new ArgumentException("Pretul minim nu poate fi mai mare decat cel maxim");
            }

            return _productRepository.GetProductsByFilters(minPrice, maxPrice, inStock, categoryId).ToList();
        }
    }
}
