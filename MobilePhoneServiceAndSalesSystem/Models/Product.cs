using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilePhoneServiceAndSalesSystem.Models
{

    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
