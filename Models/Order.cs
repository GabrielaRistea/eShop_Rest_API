using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string statusOrder { get; set; }
        public float TotalAmount { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        [ForeignKey(nameof(User))]
        public string IdUser { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(HistoryOrder))]
        public int? IdHistoryOrders { get; set; }
        public HistoryOrders? HistoryOrder { get; set; }
        public string Address { get; set; }
    }
}
