using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Phone
    {
        [Key]
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IMEI { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public string OperatingSystem { get; set; } = string.Empty;

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
