using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class RepairJob
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;
        [EnumDataType(typeof(RepairStatus))]
        public RepairStatus Status { get; set; }
        [Required]
        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        [Range(0, 100000)]
        public decimal LaborCost { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("Phone")]
        [Range(1, int.MaxValue)]
        public int PhoneId { get; set; }
        public virtual Phone? Phone { get; set; }

        [ForeignKey("Technician")]
        [Range(1, int.MaxValue)]
        public int TechnicianId { get; set; }
        public virtual Technician? Technician { get; set; }

        public virtual ICollection<SparePart> UsedParts { get; set; } = new List<SparePart>();
    }
}
