using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Phone
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string IMEI { get; set; } = string.Empty;

        [Range(1990, 2100)]
        public int YearOfManufacture { get; set; }

        [Required]
        [StringLength(100)]
        public string OperatingSystem { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("Customer")]
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
