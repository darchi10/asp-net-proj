using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize(Roles = "Admin")]
    public class OrdersApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public OrdersApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<OrderListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.Orders
                .Where(o => !o.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(o =>
                    o.ShippingAddress.Contains(term)
                    || (o.Customer != null
                        && (o.Customer.FirstName + " " + o.Customer.LastName).Contains(term)));
            }

            var orders = query
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToList()
                .Select(o => o.ToListDto())
                .ToList();

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public ActionResult<OrderDetailsDto> Get(int id)
        {
            var order = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<OrderDetailsDto> Post([FromBody] OrderDto dto)
        {
            if (!CustomerExists(dto.CustomerId))
            {
                return BadRequest("Customer does not exist.");
            }

            var orderItems = NormalizeOrderItems(dto.OrderItems);
            if (!orderItems.Any())
            {
                return BadRequest("Add at least one product.");
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount, out var error))
            {
                return BadRequest(error);
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };

            order.ApplyDto(dto);

            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = order.Id }, order.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<OrderDetailsDto> Put(int id, [FromBody] OrderDto dto)
        {
            if (!CustomerExists(dto.CustomerId))
            {
                return BadRequest("Customer does not exist.");
            }

            var order = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            var orderItems = NormalizeOrderItems(dto.OrderItems);
            if (!orderItems.Any())
            {
                return BadRequest("Add at least one product.");
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount, out var error))
            {
                return BadRequest(error);
            }

            order.ApplyDto(dto);
            order.TotalAmount = totalAmount;

            _dbContext.OrderItems.RemoveRange(order.OrderItems);
            order.OrderItems = orderItems;

            _dbContext.SaveChanges();

            return Ok(order.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var order = _dbContext.Orders
                .FirstOrDefault(o => o.Id == id && !o.IsDeleted);

            if (order is null)
            {
                return NotFound();
            }

            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }

        private bool CustomerExists(int customerId)
        {
            return _dbContext.Customers.Any(c => c.Id == customerId && !c.IsDeleted);
        }

        private static List<OrderItem> NormalizeOrderItems(IEnumerable<OrderItemDto>? items)
        {
            return items?
                .Where(i => i.ProductId > 0 && i.Quantity > 0)
                .Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                })
                .ToList() ?? new List<OrderItem>();
        }

        private bool TryApplyProductPricing(List<OrderItem> items, out decimal totalAmount, out string? error)
        {
            totalAmount = 0m;
            error = null;

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = _dbContext.Products
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .Select(p => new { p.Id, p.CurrentPrice, p.StockQuantity, p.Name })
                .ToDictionary(p => p.Id, p => p);

            foreach (var item in items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    error = "One or more selected products are invalid.";
                    return false;
                }

                if (item.Quantity > product.StockQuantity)
                {
                    error = $"'{product.Name}' has only {product.StockQuantity} in stock.";
                    return false;
                }

                item.UnitPrice = product.CurrentPrice;
                totalAmount += product.CurrentPrice * item.Quantity;
            }

            return true;
        }
    }
}
