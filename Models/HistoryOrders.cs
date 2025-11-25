using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class HistoryOrders
    {
        [Key]
        public int Id { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
