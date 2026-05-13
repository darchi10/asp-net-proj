using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Range(0.01, 100000)]
        public decimal UnitPrice { get; set; }

        [ForeignKey("Product")]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [ForeignKey("Order")]
        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }
    }
}
