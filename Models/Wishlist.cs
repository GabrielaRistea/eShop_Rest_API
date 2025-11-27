using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class Wishlist
    {
        [Key]
        public int WishlistID { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<WishlistProduct> WishlistProducts { get; set; }
    }
}
