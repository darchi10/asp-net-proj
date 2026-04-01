using System;
using System.Collections.Generic;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
