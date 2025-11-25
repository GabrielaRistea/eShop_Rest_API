using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int? Stock { get; set; }
        public byte[]? ProductImage { get; set; }
        [DataType(DataType.Upload)]
        [DisplayName("Imagine")]
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        [ForeignKey(nameof(Category))]
        public int CatogoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<WishlistProduct> WishlistProducts { get; set; }
    }
}
