using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }
        public int Quantity { get; set; }
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [ForeignKey(nameof(Order))]
        public int? IdOrder { get; set; }
        public Order Order { get; set; }
    }
}
