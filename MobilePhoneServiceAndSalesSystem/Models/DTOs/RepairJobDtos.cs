using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class RepairJobDto
    {
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

        [Range(1, int.MaxValue)]
        public int PhoneId { get; set; }

        [Range(1, int.MaxValue)]
        public int TechnicianId { get; set; }

        public List<int> UsedPartIds { get; set; } = new();
    }

    public sealed class RepairJobListDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public RepairStatus Status { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal LaborCost { get; set; }
        public int PhoneId { get; set; }
        public string PhoneLabel { get; set; } = string.Empty;
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public int UsedPartsCount { get; set; }
    }

    public sealed class RepairJobDetailsDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public RepairStatus Status { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal LaborCost { get; set; }
        public int PhoneId { get; set; }
        public string PhoneLabel { get; set; } = string.Empty;
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public List<RepairJobSparePartSummaryDto> UsedParts { get; set; } = new();
    }

    public sealed class RepairJobSparePartSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public static class RepairJobMappings
    {
        public static RepairJobListDto ToListDto(this RepairJob job)
        {
            var phoneLabel = job.Phone == null
                ? string.Empty
                : string.Concat(job.Phone.Brand, " ", job.Phone.Model, " (", job.Phone.IMEI, ")");

            var technicianName = job.Technician == null
                ? string.Empty
                : string.Concat(job.Technician.FirstName, " ", job.Technician.LastName).Trim();

            return new RepairJobListDto
            {
                Id = job.Id,
                Description = job.Description,
                Status = job.Status,
                ReceivedDate = job.ReceivedDate,
                CompletedDate = job.CompletedDate,
                LaborCost = job.LaborCost,
                PhoneId = job.PhoneId,
                PhoneLabel = phoneLabel,
                TechnicianId = job.TechnicianId,
                TechnicianName = technicianName,
                UsedPartsCount = job.UsedParts?.Count ?? 0
            };
        }

        public static RepairJobDetailsDto ToDetailsDto(this RepairJob job)
        {
            var phoneLabel = job.Phone == null
                ? string.Empty
                : string.Concat(job.Phone.Brand, " ", job.Phone.Model, " (", job.Phone.IMEI, ")");

            var technicianName = job.Technician == null
                ? string.Empty
                : string.Concat(job.Technician.FirstName, " ", job.Technician.LastName).Trim();

            return new RepairJobDetailsDto
            {
                Id = job.Id,
                Description = job.Description,
                Status = job.Status,
                ReceivedDate = job.ReceivedDate,
                CompletedDate = job.CompletedDate,
                LaborCost = job.LaborCost,
                PhoneId = job.PhoneId,
                PhoneLabel = phoneLabel,
                TechnicianId = job.TechnicianId,
                TechnicianName = technicianName,
                UsedParts = job.UsedParts.Select(p => new RepairJobSparePartSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Manufacturer = p.Manufacturer,
                    Price = p.Price
                }).ToList()
            };
        }

        public static void ApplyDto(this RepairJob job, RepairJobDto dto)
        {
            job.Description = dto.Description;
            job.Status = dto.Status;
            job.ReceivedDate = dto.ReceivedDate;
            job.CompletedDate = dto.CompletedDate;
            job.LaborCost = dto.LaborCost;
            job.PhoneId = dto.PhoneId;
            job.TechnicianId = dto.TechnicianId;
        }
    }
}
