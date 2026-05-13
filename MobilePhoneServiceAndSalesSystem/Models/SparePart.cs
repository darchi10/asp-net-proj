using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class SparePart
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Manufacturer { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
