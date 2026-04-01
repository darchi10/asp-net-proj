using System;
using System.Collections.Generic;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Technician
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }

        public List<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
    }
}
