using System.Collections.Generic;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Phone
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IMEI { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public string OperatingSystem { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public List<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
