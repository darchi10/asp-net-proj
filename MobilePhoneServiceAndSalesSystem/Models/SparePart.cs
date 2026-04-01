using System.Collections.Generic;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class SparePart
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public List<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
