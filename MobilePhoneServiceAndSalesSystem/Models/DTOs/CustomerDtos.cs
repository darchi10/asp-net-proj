using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class CustomerDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;
    }

    public sealed class CustomerListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public int PhonesCount { get; set; }
    }

    public sealed class CustomerDetailsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<OrderSummaryDto> Orders { get; set; } = new();
        public List<PhoneSummaryDto> Phones { get; set; } = new();
    }

    public sealed class OrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public sealed class PhoneSummaryDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IMEI { get; set; } = string.Empty;
    }

    public static class CustomerMappings
    {
        public static CustomerListDto ToListDto(this Customer customer)
        {
            return new CustomerListDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                OrdersCount = customer.Orders?.Count ?? 0,
                PhonesCount = customer.Phones?.Count ?? 0
            };
        }

        public static CustomerDetailsDto ToDetailsDto(this Customer customer)
        {
            return new CustomerDetailsDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                Orders = customer.Orders.Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount
                }).ToList(),
                Phones = customer.Phones.Select(p => new PhoneSummaryDto
                {
                    Id = p.Id,
                    Brand = p.Brand,
                    Model = p.Model,
                    IMEI = p.IMEI
                }).ToList()
            };
        }

        public static void ApplyDto(this Customer customer, CustomerDto dto)
        {
            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.Email = dto.Email;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.Address = dto.Address;
        }
    }
}
