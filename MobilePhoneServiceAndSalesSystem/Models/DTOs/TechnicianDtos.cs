using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class TechnicianDto
    {
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
    }

    public sealed class TechnicianListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public int RepairJobsCount { get; set; }
    }

    public sealed class TechnicianDetailsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public List<TechnicianRepairJobSummaryDto> RepairJobs { get; set; } = new();
    }

    public sealed class TechnicianRepairJobSummaryDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public RepairStatus Status { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal LaborCost { get; set; }
        public int PhoneId { get; set; }
    }

    public static class TechnicianMappings
    {
        public static TechnicianListDto ToListDto(this Technician technician)
        {
            return new TechnicianListDto
            {
                Id = technician.Id,
                FirstName = technician.FirstName,
                LastName = technician.LastName,
                Specialization = technician.Specialization,
                HireDate = technician.HireDate,
                Salary = technician.Salary,
                RepairJobsCount = technician.RepairJobs?.Count ?? 0
            };
        }

        public static TechnicianDetailsDto ToDetailsDto(this Technician technician)
        {
            return new TechnicianDetailsDto
            {
                Id = technician.Id,
                FirstName = technician.FirstName,
                LastName = technician.LastName,
                Specialization = technician.Specialization,
                HireDate = technician.HireDate,
                Salary = technician.Salary,
                RepairJobs = technician.RepairJobs.Select(r => new TechnicianRepairJobSummaryDto
                {
                    Id = r.Id,
                    Description = r.Description,
                    Status = r.Status,
                    ReceivedDate = r.ReceivedDate,
                    CompletedDate = r.CompletedDate,
                    LaborCost = r.LaborCost,
                    PhoneId = r.PhoneId
                }).ToList()
            };
        }

        public static void ApplyDto(this Technician technician, TechnicianDto dto)
        {
            technician.FirstName = dto.FirstName;
            technician.LastName = dto.LastName;
            technician.Specialization = dto.Specialization;
            technician.HireDate = dto.HireDate;
            technician.Salary = dto.Salary;
        }
    }
}
