using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using UglyToad.PdfPig;
using System.IO;
using System;
using System.Collections.Generic;
using Version = Lucene.Net.Util.Version;
using Proiect.Models;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Proiect.Services.Interfaces;
using Proiect.DTOs;

namespace Proiect.Services
{
    public class LuceneService : ILuceneService
    {
        private readonly string _indexPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "LuceneIndex");

        private readonly IProductService _productService;

        public LuceneService(IProductService productService)
        {
            _productService = productService;
        }

        public void BuildIndex(List<Product> products)
        {
            var dirInfo = new DirectoryInfo(_indexPath);
            using var dir = FSDirectory.Open(dirInfo);

            using var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using var writer = new IndexWriter(dir, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            foreach (var product in products)
            {
                if (string.IsNullOrEmpty(product.PdfPath)) continue;

                string fullPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", product.PdfPath.TrimStart('/'));
                if (!File.Exists(fullPath)) continue;

                try
                {
                    using var pdf = PdfDocument.Open(fullPath);
                    string pdfText = "";
                    foreach (var page in pdf.GetPages())
                    {
                        pdfText += page.Text + " ";
                    }

                    var doc = new Document();

                    doc.Add(new Field("ProductId", product.ProductID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                    doc.Add(new Field("Content", pdfText, Field.Store.NO, Field.Index.ANALYZED));

                    writer.AddDocument(doc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare indexare PDF {product.Name}: {ex.Message}");
                }
            }

            writer.Optimize();
            writer.Commit();
        }


        public List<AdvancedSearchResultDto> Search(string searchQuery, string sortOrder, int maxResults = 10)
        {
            var finalResults = new List<AdvancedSearchResultDto>();

            if (string.IsNullOrWhiteSpace(searchQuery))
                return finalResults;

            var dirInfo = new System.IO.DirectoryInfo(_indexPath);

            if (!System.IO.Directory.Exists(_indexPath) || dirInfo.GetFiles().Length == 0)
                return finalResults;

            using var dir = FSDirectory.Open(dirInfo);
            using var searcher = new IndexSearcher(dir, true);
            using var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            try
            {
                var parser = new QueryParser(Version.LUCENE_30, "Content" , analyzer);
                var query = parser.Parse(searchQuery);

                TopDocs topDocs = searcher.Search(query, maxResults);

                foreach (var scoreDoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(scoreDoc.Doc);
                    int productId = int.Parse(doc.Get("ProductId"));
                    float score = scoreDoc.Score;

                    var product = _productService.GetProductById(productId);
                    if (product != null)
                    {
                        finalResults.Add(new AdvancedSearchResultDto
                        {
                            ProductID = product.ProductID,
                            Name = product.Name,
                            Price = product.Price,
                            ProductImage = product.ProductImage,
                            LuceneScore = score
                        });
                    }
                }
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Eroare parsare query Lucene: {ex.Message}");
            }

            if (sortOrder.ToLower() == "asc")
            {
                return finalResults.OrderBy(x => x.LuceneScore).ToList();
            }

            return finalResults.OrderByDescending(x => x.LuceneScore).ToList();
        }
    }
}
