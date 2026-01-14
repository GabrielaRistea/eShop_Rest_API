namespace Proiect.DTOs
{
    public class CartItemDisplayDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public float PricePerUnit { get; set; }
        public float Subtotal { get; set; }
        public byte[] ProductImage { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
