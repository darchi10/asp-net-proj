using System;
using System.Collections.Generic;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class RepairJob
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public RepairStatus Status { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal LaborCost { get; set; }

        public int PhoneId { get; set; }
        public Phone? Phone { get; set; }

        public int TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public List<SparePart> UsedParts { get; set; } = new List<SparePart>();
    }
}
