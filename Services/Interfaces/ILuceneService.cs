using Proiect.DTOs;
using Proiect.Models;

namespace Proiect.Services.Interfaces
{
    public interface ILuceneService
    {
        void BuildIndex(List<Product> products);
        List<AdvancedSearchResultDto> Search(string searchQuery, string sortOrder, int maxResults = 10);
    }
}
