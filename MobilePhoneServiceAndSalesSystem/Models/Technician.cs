using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Technician
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        public DateTime HireDate { get; set; }

        [Range(0, 100000)]
        public decimal Salary { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
