namespace MobilePhoneServiceAndSalesSystem.Models
{
    using System.Collections.Generic;

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
