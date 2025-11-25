namespace Proiect.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int? Stock { get; set; }
        public byte[]? ProductImage { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int Category { get; set; }
        public string? CategoryName { get; set; }
    }
}
