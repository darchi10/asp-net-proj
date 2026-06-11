using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class SparePartDto
    {
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
    }

    public sealed class SparePartListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int RepairJobsCount { get; set; }
    }

    public sealed class SparePartDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public List<SparePartRepairJobSummaryDto> RepairJobs { get; set; } = new();
    }

    public sealed class SparePartRepairJobSummaryDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public RepairStatus Status { get; set; }
        public int PhoneId { get; set; }
        public int TechnicianId { get; set; }
    }

    public static class SparePartMappings
    {
        public static SparePartListDto ToListDto(this SparePart sparePart)
        {
            return new SparePartListDto
            {
                Id = sparePart.Id,
                Name = sparePart.Name,
                Manufacturer = sparePart.Manufacturer,
                Price = sparePart.Price,
                StockQuantity = sparePart.StockQuantity,
                RepairJobsCount = sparePart.RepairJobs?.Count ?? 0
            };
        }

        public static SparePartDetailsDto ToDetailsDto(this SparePart sparePart)
        {
            return new SparePartDetailsDto
            {
                Id = sparePart.Id,
                Name = sparePart.Name,
                Manufacturer = sparePart.Manufacturer,
                Price = sparePart.Price,
                StockQuantity = sparePart.StockQuantity,
                RepairJobs = sparePart.RepairJobs.Select(r => new SparePartRepairJobSummaryDto
                {
                    Id = r.Id,
                    Description = r.Description,
                    Status = r.Status,
                    PhoneId = r.PhoneId,
                    TechnicianId = r.TechnicianId
                }).ToList()
            };
        }

        public static void ApplyDto(this SparePart sparePart, SparePartDto dto)
        {
            sparePart.Name = dto.Name;
            sparePart.Manufacturer = dto.Manufacturer;
            sparePart.Price = dto.Price;
            sparePart.StockQuantity = dto.StockQuantity;
        }
    }
}
