using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class OrderDto
    {
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public sealed class OrderItemDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }
    }

    public sealed class OrderListDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
    }

    public sealed class OrderDetailsDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItemDetailsDto> OrderItems { get; set; } = new();
    }

    public sealed class OrderItemDetailsDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public static class OrderMappings
    {
        public static OrderListDto ToListDto(this Order order)
        {
            var customerName = order.Customer == null
                ? string.Empty
                : string.Concat(order.Customer.FirstName, " ", order.Customer.LastName).Trim();

            return new OrderListDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerId = order.CustomerId,
                CustomerName = customerName,
                ItemsCount = order.OrderItems?.Count ?? 0
            };
        }

        public static OrderDetailsDto ToDetailsDto(this Order order)
        {
            var customerName = order.Customer == null
                ? string.Empty
                : string.Concat(order.Customer.FirstName, " ", order.Customer.LastName).Trim();

            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerId = order.CustomerId,
                CustomerName = customerName,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDetailsDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        public static void ApplyDto(this Order order, OrderDto dto)
        {
            order.CustomerId = dto.CustomerId;
            order.ShippingAddress = dto.ShippingAddress;
        }
    }
}
