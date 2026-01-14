using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class HistoryOrders
    {
        [Key]
        public int Id { get; set; }
        public ICollection<Order> Orders { get; set; }
        [ForeignKey(nameof(User))]
        public string IdUser { get; set; }
        public User User { get; set; }
    }
}
