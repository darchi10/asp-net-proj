using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class PhoneAiDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Imei { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public string OperatingSystem { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
    }

    public sealed class PhoneDto
    {
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

        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }
    }

    public sealed class PhoneListDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IMEI { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public string OperatingSystem { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int RepairJobsCount { get; set; }
    }

    public sealed class PhoneDetailsDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IMEI { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public string OperatingSystem { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<RepairJobSummaryDto> RepairJobs { get; set; } = new();
    }

    public sealed class RepairJobSummaryDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public RepairStatus Status { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal LaborCost { get; set; }
        public int TechnicianId { get; set; }
    }

    public static class PhoneMappings
    {
        public static PhoneListDto ToListDto(this Phone phone)
        {
            var customerName = phone.Customer == null
                ? string.Empty
                : string.Concat(phone.Customer.FirstName, " ", phone.Customer.LastName).Trim();

            return new PhoneListDto
            {
                Id = phone.Id,
                Brand = phone.Brand,
                Model = phone.Model,
                IMEI = phone.IMEI,
                YearOfManufacture = phone.YearOfManufacture,
                OperatingSystem = phone.OperatingSystem,
                CustomerId = phone.CustomerId,
                CustomerName = customerName,
                RepairJobsCount = phone.RepairJobs?.Count ?? 0
            };
        }

        public static PhoneDetailsDto ToDetailsDto(this Phone phone)
        {
            var customerName = phone.Customer == null
                ? string.Empty
                : string.Concat(phone.Customer.FirstName, " ", phone.Customer.LastName).Trim();

            return new PhoneDetailsDto
            {
                Id = phone.Id,
                Brand = phone.Brand,
                Model = phone.Model,
                IMEI = phone.IMEI,
                YearOfManufacture = phone.YearOfManufacture,
                OperatingSystem = phone.OperatingSystem,
                CustomerId = phone.CustomerId,
                CustomerName = customerName,
                RepairJobs = phone.RepairJobs.Select(r => new RepairJobSummaryDto
                {
                    Id = r.Id,
                    Description = r.Description,
                    Status = r.Status,
                    ReceivedDate = r.ReceivedDate,
                    CompletedDate = r.CompletedDate,
                    LaborCost = r.LaborCost,
                    TechnicianId = r.TechnicianId
                }).ToList()
            };
        }

        public static void ApplyDto(this Phone phone, PhoneDto dto)
        {
            phone.Brand = dto.Brand;
            phone.Model = dto.Model;
            phone.IMEI = dto.IMEI;
            phone.YearOfManufacture = dto.YearOfManufacture;
            phone.OperatingSystem = dto.OperatingSystem;
            phone.CustomerId = dto.CustomerId;
        }
    }
}
