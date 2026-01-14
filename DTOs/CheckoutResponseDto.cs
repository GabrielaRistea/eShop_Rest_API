namespace Proiect.DTOs
{
    public class CheckoutResponseDto
    {
        public bool RedirectToStripe { get; set; }
        public string StripeUrl { get; set; }
        public int OrderId { get; set; }
    }
}
