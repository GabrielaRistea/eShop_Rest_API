namespace Proiect.DTOs
{
    public class AdvancedSearchResultDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public byte[] ProductImage { get; set; }
        public float LuceneScore { get; set; }
    }
}
