using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public sealed class AiParseRequest
    {
        public string Input { get; set; } = string.Empty;
    }

    public sealed class ProductDto
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal CurrentPrice { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; }
    }

    public sealed class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }
        public int OrderItemsCount { get; set; }
    }

    public sealed class ProductDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }
        public List<ProductOrderItemSummaryDto> OrderItems { get; set; } = new();
    }

    public sealed class ProductOrderItemSummaryDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public static class ProductMappings
    {
        public static ProductListDto ToListDto(this Product product)
        {
            return new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CurrentPrice = product.CurrentPrice,
                StockQuantity = product.StockQuantity,
                OrderItemsCount = product.OrderItems?.Count ?? 0
            };
        }

        public static ProductDetailsDto ToDetailsDto(this Product product)
        {
            return new ProductDetailsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CurrentPrice = product.CurrentPrice,
                StockQuantity = product.StockQuantity,
                OrderItems = product.OrderItems.Select(oi => new ProductOrderItemSummaryDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        public static void ApplyDto(this Product product, ProductDto dto)
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CurrentPrice = dto.CurrentPrice;
            product.StockQuantity = dto.StockQuantity;
        }
    }
}
