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
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ToList();

            return View(orders);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
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

            return View(order);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var orders = _dbContext.Orders
                .Where(o => !o.IsDeleted
                    && (o.ShippingAddress + " " + o.Customer.FirstName + " " + o.Customer.LastName).Contains(query))
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ToList();

            return PartialView("_OrderCards", orders);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            PopulateCustomerSelection(null);
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
                PopulateCustomerSelection(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                return View(order);
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount))
            {
                PopulateCustomerSelection(order.CustomerId);
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
                .Where(o => !o.IsDeleted)
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            PopulateCustomerSelection(order.CustomerId);
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
                .Where(o => !o.IsDeleted)
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
                PopulateCustomerSelection(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                order.OrderDate = existingOrder.OrderDate;
                return View(order);
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount))
            {
                PopulateCustomerSelection(order.CustomerId);
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

        [HttpGet]
        [Route("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var order = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = order.OrderItems.Any();
            return View(order);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var order = _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id && !o.IsDeleted);

            if (order is null)
            {
                return NotFound();
            }

            var hasDependencies = order.OrderItems.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                order.IsDeleted = true;
                order.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (order.OrderItems.Any())
            {
                _dbContext.OrderItems.RemoveRange(order.OrderItems);
            }

            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateCustomerSelection(int? selectedCustomerId)
        {
            ViewBag.SelectedCustomerText = string.Empty;
            if (!selectedCustomerId.HasValue || selectedCustomerId.Value <= 0)
            {
                return;
            }

            var label = _dbContext.Customers
                .Where(c => !c.IsDeleted && c.Id == selectedCustomerId.Value)
                .Select(c => c.FirstName + " " + c.LastName)
                .FirstOrDefault();

            ViewBag.SelectedCustomerText = label ?? string.Empty;
        }

        private void PopulateProducts()
        {
            var products = _dbContext.Products
                .Where(p => !p.IsDeleted)
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
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .Select(p => new { p.Id, p.CurrentPrice, p.StockQuantity, p.Name })
                .ToDictionary(p => p.Id, p => p);

            foreach (var item in items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    ModelState.AddModelError("OrderItems", "One or more selected products are invalid.");
                    return false;
                }

                if (item.Quantity > product.StockQuantity)
                {
                    ModelState.AddModelError(
                        "OrderItems",
                        $"'{product.Name}' has only {product.StockQuantity} in stock.");
                    return false;
                }

                item.UnitPrice = product.CurrentPrice;
                totalAmount += product.CurrentPrice * item.Quantity;
            }

            return true;
        }
    }
}