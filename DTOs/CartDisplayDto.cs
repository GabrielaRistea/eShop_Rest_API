namespace Proiect.DTOs
{
    public class CartDisplayDto
    {
        public List<CartItemDisplayDto> Items { get; set; }
        public float TotalAmount { get; set; }
    }
}
