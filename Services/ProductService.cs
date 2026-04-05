using Humanizer;
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

            if (scoredResults.Count > 0)
            {
                return scoredResults
                .OrderByDescending(r => r.Score)
                .Select(r => r.Product)
                .ToList();
            }

            // rezultate pentru cautare cu cuvinte gresite
            var suggestions = GetPredictiveSuggestions(query);
            
            return allProducts.Where(p => suggestions.Contains(p.Name))
                              .ToList();
            
        }

        private int CalculateLevenshteinDistance(string s, string t)
        {
            int n = s.Length;   // lungime text cautat
            int m = t.Length;   // lungime text cu care se compara (numele produsului)
            int[,] d = new int[n + 1, m + 1];   // creare matrice pentru a stoca rezultatele

            if (n == 0) return n;
            if (m == 0) return m;

            // initializare linii si coloane 
            for (int i = 0; i <= n; d[i, 0] = i++) ; 
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),     // stergere sau inserare
                        d[i - 1, j - 1] + cost);                        // inlocuire
                }
            }

            return d[n, m];
        }

        public List<string> GetPredictiveSuggestions(string query)
        {
            var allProducts = _productRepository.GetAll().ToList();
            if (string.IsNullOrWhiteSpace(query)) return new List<string>();

            string lowerQuery = query.ToLower();
            var suggestions = new List<(string Name, int Distance, bool IsPrefix)>();

            foreach (var product in allProducts)
            {
                if (string.IsNullOrEmpty(product.Name)) continue;
                string name = product.Name.ToLower();

                if (name.Contains(lowerQuery))
                {
                    bool startsWith = name.StartsWith(lowerQuery);
                    suggestions.Add((product.Name, 0, startsWith));
                    continue;
                }

                int fullDistance = CalculateLevenshteinDistance(lowerQuery, name);

                var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int minWordDistance = words.Select(w => CalculateLevenshteinDistance(lowerQuery, w)).Min();

                int finalDistance = Math.Min(fullDistance, minWordDistance);

                int allowedDistance = lowerQuery.Length <= 3 ? 1 : 2;

                // maxim doua greseli
                if (finalDistance <= allowedDistance)
                {
                    suggestions.Add((product.Name, finalDistance, false));
                }
            }

            return suggestions
                .OrderBy(x => x.Distance)
                .ThenByDescending(x => x.IsPrefix)
                .Select(x => x.Name)
                .Distinct()
                .Take(5)
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

        public List<ProductDto> GetSimilarProducts(int productId, int limit = 4)
        {
            var allProducts = _productRepository.GetAll().ToList();
            var targetProduct = allProducts.FirstOrDefault(p => p.ProductID == productId); 

            if (targetProduct == null) return new List<ProductDto>();

            var otherProducts = allProducts.Where(p => p.ProductID != productId).ToList();

            Func<Product, string[]> GetWords = p => ((p.Name ?? "") + " " + (p.Description ?? ""))
                .ToLower()
                .Split(new[] { ' ', ',', '.', '!', '?', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var targetWords = GetWords(targetProduct);
            var allDocs = allProducts.Select(p => GetWords(p)).ToList();

            var idfWeights = new Dictionary<string, double>();

            var targetVector = new Dictionary<string, double>();
            double targetMagnitudeSq = 0; 

            foreach (var word in targetWords.Distinct())
            {
                int docsWithWord = allDocs.Count(d => d.Contains(word));
                double idf = Math.Log((double)allDocs.Count / (docsWithWord > 0 ? docsWithWord : 1) + 1.0);
                idfWeights[word] = idf;

                double tf = (double)targetWords.Count(w => w == word) / targetWords.Length;
                double val = tf * idf;

                targetVector[word] = val;
                targetMagnitudeSq += val * val;
            }

            double targetMagnitude = Math.Sqrt(targetMagnitudeSq);
            if (targetMagnitude == 0) return new List<ProductDto>();

            var scoredProducts = new List<(Product Product, double Score)>();

            foreach (var product in otherProducts)
            {
                var words = GetWords(product);
                if (words.Length == 0) continue;

                double dotProduct = 0;
                double otherMagnitudeSq = 0; 

                foreach (var word in words.Distinct())
                {
                    if (!idfWeights.ContainsKey(word))
                    {
                        int docsWithWord = allDocs.Count(d => d.Contains(word));
                        idfWeights[word] = Math.Log((double)allDocs.Count / (docsWithWord > 0 ? docsWithWord : 1) + 1.0);
                    }

                    double tf = (double)words.Count(w => w == word) / words.Length;
                    double val = tf * idfWeights[word];
                    otherMagnitudeSq += val * val;

                    if (targetVector.ContainsKey(word))
                    {
                        dotProduct += targetVector[word] * val;
                    }
                }

                double otherMagnitude = Math.Sqrt(otherMagnitudeSq);

                if (otherMagnitude > 0)
                {
                    double cosineSimilarity = dotProduct / (targetMagnitude * otherMagnitude);
                    if (cosineSimilarity > 0) 
                    {
                        scoredProducts.Add((product, cosineSimilarity));
                    }
                }
            }

            return scoredProducts
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .Select(x => new ProductDto 
                {
                    Id = x.Product.ProductID,
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    Description = x.Product.Description,
                    ProductImage = x.Product.ProductImage
                })
                .ToList();
        }
    }
}
