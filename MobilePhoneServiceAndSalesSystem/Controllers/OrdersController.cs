using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Security.Claims;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("orders")]
    [Authorize(Roles = "Admin,Customer")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public OrdersController(AppDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        [Route("")]
        public IActionResult Index()
        {
            var ordersQuery = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admin sees everything
            }
            else if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue)
                {
                    return Forbid();
                }

                ordersQuery = ordersQuery.Where(o => o.CustomerId == customerId.Value);
            }

            var orders = ordersQuery.ToList();

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
                .Include(o => o.Attachments)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                // Admin sees everything
            }
            else if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue || order.CustomerId != customerId.Value)
                {
                    return Forbid();
                }
            }

            return View(order);
        }

        [HttpGet]
        [Route("{id:int}/files")]
        public IActionResult GetFiles(int id)
        {
            var order = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.Attachments)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                // Admin sees everything
            }
            else if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue || order.CustomerId != customerId.Value)
                {
                    return Forbid();
                }
            }

            var files = order.Attachments
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new
                {
                    a.Id,
                    a.OriginalFileName,
                    a.FileSize,
                    a.ContentType,
                    a.UploadedAt,
                    a.FilePath
                })
                .ToList();

            return Json(files);
        }

        [HttpPost]
        [Route("{id:int}/files")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            var order = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No file selected." });
            }

            const long maxSize = 10 * 1024 * 1024;
            if (file.Length > maxSize)
            {
                return BadRequest(new { message = "File size must be 10 MB or less." });
            }

            var extension = Path.GetExtension(file.FileName);
            var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg" };
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Only PDF and image files are allowed." });
            }

            var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "orders", id.ToString());
            Directory.CreateDirectory(uploadRoot);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadRoot, storedFileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/orders/{id}/{storedFileName}";
            var attachment = new OrderAttachment
            {
                OrderId = id,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType ?? string.Empty,
                FileSize = file.Length,
                FilePath = relativePath,
                UploadedAt = DateTime.UtcNow
            };

            _dbContext.OrderAttachments.Add(attachment);
            _dbContext.SaveChanges();

            return Ok(new
            {
                attachment.Id,
                attachment.OriginalFileName,
                attachment.FileSize,
                attachment.ContentType,
                attachment.UploadedAt,
                attachment.FilePath
            });
        }

        [HttpDelete]
        [Route("files/{fileId:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteFile(int fileId)
        {
            var attachment = _dbContext.OrderAttachments
                .Include(a => a.Order)
                .FirstOrDefault(a => a.Id == fileId);

            if (attachment is null || attachment.Order?.IsDeleted == true)
            {
                return NotFound();
            }

            var absolutePath = Path.Combine(_environment.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }

            _dbContext.OrderAttachments.Remove(attachment);
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var ordersQuery = _dbContext.Orders
                .Where(o => !o.IsDeleted
                    && (o.ShippingAddress.Contains(query) || (o.Customer != null && (o.Customer.FirstName + " " + o.Customer.LastName).Contains(query))))
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue)
                {
                    return Forbid();
                }

                ordersQuery = ordersQuery.Where(o => o.CustomerId == customerId.Value);
            }

            var orders = ordersQuery.ToList();

            return PartialView("_OrderCards", orders);
        }

        [HttpGet]
        [Route("create")]
        [Authorize(Roles = "Admin,Customer")]
        public IActionResult Create()
        {
            if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue)
                {
                    return Forbid();
                }
                PopulateCustomerSelection(customerId.Value);
                PopulateProducts();
                return View(new Order { CustomerId = customerId.Value });
            }

            PopulateCustomerSelection(null);
            PopulateProducts();
            return View();
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin,Customer")]
        public IActionResult Create(Order order)
        {
            if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue)
                {
                    return Forbid();
                }
                order.CustomerId = customerId.Value; // Force link to logged-in user
            }

            var orderItems = NormalizeOrderItems(order.OrderItems);

            if (!orderItems.Any())
            {
                ModelState.AddModelError("OrderItems", "Add at least one product.");
            }

            ModelState.Remove("Customer");

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

            // Reduce product stock quantity
            foreach (var item in orderItems)
            {
                var product = _dbContext.Products.Find(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

            // Temporarily revert old order items back to product stock for validation
            foreach (var oldItem in existingOrder.OrderItems)
            {
                var product = _dbContext.Products.Find(oldItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity += oldItem.Quantity;
                }
            }

            if (!ModelState.IsValid)
            {
                // Restore stock back since we are returning the view
                foreach (var oldItem in existingOrder.OrderItems)
                {
                    var product = _dbContext.Products.Find(oldItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= oldItem.Quantity;
                    }
                }

                PopulateCustomerSelection(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                order.OrderDate = existingOrder.OrderDate;
                return View(order);
            }

            if (!TryApplyProductPricing(orderItems, out var totalAmount))
            {
                // Restore stock back since we are returning the view
                foreach (var oldItem in existingOrder.OrderItems)
                {
                    var product = _dbContext.Products.Find(oldItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= oldItem.Quantity;
                    }
                }

                PopulateCustomerSelection(order.CustomerId);
                PopulateProducts();
                order.OrderItems = orderItems;
                order.OrderDate = existingOrder.OrderDate;
                return View(order);
            }

            // Apply new stock reduction
            foreach (var newItem in orderItems)
            {
                var product = _dbContext.Products.Find(newItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= newItem.Quantity;
                }
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var order = _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Attachments)
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

            // Revert stock back for deleted order items
            foreach (var item in order.OrderItems)
            {
                var product = _dbContext.Products.Find(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
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

            if (order.Attachments.Any())
            {
                foreach (var attachment in order.Attachments)
                {
                    var absolutePath = Path.Combine(_environment.WebRootPath, attachment.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(absolutePath))
                    {
                        System.IO.File.Delete(absolutePath);
                    }
                }

                _dbContext.OrderAttachments.RemoveRange(order.Attachments);
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

        private int? EnsureCustomerLink()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var customer = _dbContext.Customers.FirstOrDefault(c => !c.IsDeleted && c.UserId == userId);
            if (customer != null)
            {
                return customer.Id;
            }

            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            customer = _dbContext.Customers.FirstOrDefault(c => !c.IsDeleted && c.Email == email);
            if (customer == null)
            {
                return null;
            }

            customer.UserId = userId;
            _dbContext.SaveChanges();
            return customer.Id;
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