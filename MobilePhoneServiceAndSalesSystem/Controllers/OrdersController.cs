using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("orders")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _dbContext;

        public OrdersController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var orders = _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ToList();

            return View(orders);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var order = _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            PopulateCustomers();
            PopulateProducts();
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Order order)
        {
            var orderItems = NormalizeOrderItems(order.OrderItems);

            if (!orderItems.Any())
            {
                ModelState.AddModelError("OrderItems", "Add at least one product.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCustomers(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                return View(order);
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount))
            {
                PopulateCustomers(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                return View(order);
            }

            order.OrderDate = DateTime.Now;
            order.TotalAmount = totalAmount;
            order.OrderItems = orderItems;

            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var order = _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            PopulateCustomers(order.CustomerId);
            PopulateProducts();
            return View(order);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            var existingOrder = _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (existingOrder is null)
            {
                return NotFound();
            }

            var orderItems = NormalizeOrderItems(order.OrderItems);

            if (!orderItems.Any())
            {
                ModelState.AddModelError("OrderItems", "Add at least one product.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCustomers(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                order.OrderDate = existingOrder.OrderDate;
                return View(order);
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount))
            {
                PopulateCustomers(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                order.OrderDate = existingOrder.OrderDate;
                return View(order);
            }

            existingOrder.CustomerId = order.CustomerId;
            existingOrder.ShippingAddress = order.ShippingAddress;
            existingOrder.TotalAmount = totalAmount;

            _dbContext.OrderItems.RemoveRange(existingOrder.OrderItems);
            existingOrder.OrderItems = orderItems;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateCustomers(int? selectedCustomerId = null)
        {
            var customers = _dbContext.Customers
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new { c.Id, FullName = c.FirstName + " " + c.LastName })
                .ToList();

            ViewBag.CustomerId = new SelectList(customers, "Id", "FullName", selectedCustomerId);
        }

        private void PopulateProducts()
        {
            var products = _dbContext.Products
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToList();

            ViewBag.Products = new SelectList(products, "Id", "Name");
        }

        private static List<OrderItem> NormalizeOrderItems(ICollection<OrderItem>? items)
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

        private bool TryApplyProductPricing(List<OrderItem> items, out decimal totalAmount)
        {
            totalAmount = 0m;
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p.CurrentPrice);

            foreach (var item in items)
            {
                if (!products.TryGetValue(item.ProductId, out var unitPrice))
                {
                    ModelState.AddModelError("OrderItems", "One or more selected products are invalid.");
                    return false;
                }

                item.UnitPrice = unitPrice;
                totalAmount += unitPrice * item.Quantity;
            }

            return true;
        }
    }
}