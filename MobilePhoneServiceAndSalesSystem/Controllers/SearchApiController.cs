using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [ApiController]
    [Route("api/search")]
    [AllowAnonymous]
    public class SearchApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public SearchApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SearchResultDto>>> Search([FromQuery] string? q)
        {
            var results = new List<SearchResultDto>();
            var query = q?.Trim().ToLower() ?? string.Empty;

            var isAdmin = User.IsInRole("Admin");
            var isWorker = User.IsInRole("Worker");
            var isCustomer = User.IsInRole("Customer");
            var isAuthenticated = User.Identity?.IsAuthenticated == true;

            var menuItems = GetAvailableMenuItems(isAdmin, isWorker, isCustomer, isAuthenticated);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var filteredMenus = menuItems.Where(m =>
                    m.Title.ToLower().Contains(query) ||
                    m.Description.ToLower().Contains(query) ||
                    (m.Url != null && m.Url.ToLower().Contains(query))
                ).ToList();

                results.AddRange(filteredMenus);

                if (query.Length >= 2)
                {
                    var products = await _dbContext.Products
                        .Where(p => !p.IsDeleted && (p.Name.ToLower().Contains(query) || p.Description.ToLower().Contains(query)))
                        .Take(5)
                        .Select(p => new SearchResultDto
                        {
                            Title = p.Name,
                            Description = $"Price: {p.CurrentPrice:N2} EUR | Stock: {p.StockQuantity} pcs",
                            Category = "Products",
                            Url = $"/products/{p.Id}",
                            Icon = "bi-box-seam"
                        })
                        .ToListAsync();
                    results.AddRange(products);

                    if (isAdmin)
                    {
                        var customers = await _dbContext.Customers
                            .Where(c => !c.IsDeleted && (
                                (c.FirstName + " " + c.LastName).ToLower().Contains(query) ||
                                c.Email.ToLower().Contains(query) ||
                                c.PhoneNumber.Contains(query)
                            ))
                            .Take(5)
                            .Select(c => new SearchResultDto
                            {
                                Title = $"{c.FirstName} {c.LastName}",
                                Description = $"Email: {c.Email} | Phone: {c.PhoneNumber}",
                                Category = "Customers",
                                Url = $"/customers/{c.Id}",
                                Icon = "bi-person"
                            })
                            .ToListAsync();
                        results.AddRange(customers);
                    }

                    if (isAdmin)
                    {
                        var phones = await _dbContext.Phones
                            .Where(p => !p.IsDeleted && (
                                p.Brand.ToLower().Contains(query) ||
                                p.Model.ToLower().Contains(query) ||
                                p.IMEI.Contains(query)
                            ))
                            .Take(5)
                            .Select(p => new SearchResultDto
                            {
                                Title = $"{p.Brand} {p.Model}",
                                Description = $"IMEI: {p.IMEI} | Owner: {(p.Customer != null ? p.Customer.FirstName + " " + p.Customer.LastName : "None")}",
                                Category = "Phones",
                                Url = $"/phones/{p.Id}",
                                Icon = "bi-phone"
                            })
                            .ToListAsync();
                        results.AddRange(phones);
                    }

                    if (isAdmin || isWorker)
                    {
                        var spareParts = await _dbContext.SpareParts
                            .Where(s => !s.IsDeleted && (s.Name.ToLower().Contains(query) || s.Manufacturer.ToLower().Contains(query)))
                            .Take(5)
                            .Select(s => new SearchResultDto
                            {
                                Title = s.Name,
                                Description = $"Manufacturer: {s.Manufacturer} | Price: {s.Price:N2} EUR | Stock: {s.StockQuantity}",
                                Category = "Spare Parts",
                                Url = $"/spare-parts/{s.Id}",
                                Icon = "bi-cpu"
                            })
                            .ToListAsync();
                        results.AddRange(spareParts);
                    }

                    if (isAdmin || isWorker || isCustomer)
                    {
                        var repairQuery = _dbContext.RepairJobs
                            .Where(r => !r.IsDeleted)
                            .AsQueryable();

                        if (isCustomer && !isAdmin && !isWorker)
                        {
                            var customerId = EnsureCustomerLink();
                            if (customerId.HasValue)
                            {
                                repairQuery = repairQuery.Where(r => r.Phone != null && r.Phone.CustomerId == customerId.Value);
                            }
                            else
                            {
                                repairQuery = repairQuery.Where(r => false);
                            }
                        }

                        int.TryParse(query, out int searchId);
                        repairQuery = repairQuery.Where(r =>
                            r.Id == searchId ||
                            r.Description.ToLower().Contains(query) ||
                            (r.Phone != null && (r.Phone.Brand + " " + r.Phone.Model).ToLower().Contains(query))
                        );

                        var repairs = await repairQuery
                            .Take(5)
                            .Select(r => new SearchResultDto
                            {
                                Title = $"Ticket #{r.Id} - {(r.Phone != null ? r.Phone.Brand + " " + r.Phone.Model : "Device")}",
                                Description = $"Status: {r.Status} | Details: {(r.Description.Length > 40 ? r.Description.Substring(0, 37) + "..." : r.Description)}",
                                Category = "Repairs",
                                Url = $"/repair-jobs/{r.Id}",
                                Icon = "bi-tools"
                            })
                            .ToListAsync();
                        results.AddRange(repairs);
                    }

                    if (isAdmin || isCustomer)
                    {
                        var ordersQuery = _dbContext.Orders
                            .Where(o => !o.IsDeleted)
                            .AsQueryable();

                        if (isCustomer && !isAdmin)
                        {
                            var customerId = EnsureCustomerLink();
                            if (customerId.HasValue)
                            {
                                ordersQuery = ordersQuery.Where(o => o.CustomerId == customerId.Value);
                            }
                            else
                            {
                                ordersQuery = ordersQuery.Where(o => false);
                            }
                        }

                        int.TryParse(query, out int searchOrderId);
                        ordersQuery = ordersQuery.Where(o =>
                            o.Id == searchOrderId ||
                            o.ShippingAddress.ToLower().Contains(query) ||
                            (o.Customer != null && (o.Customer.FirstName + " " + o.Customer.LastName).ToLower().Contains(query))
                        );

                        var orders = await ordersQuery
                            .Take(5)
                            .Select(o => new SearchResultDto
                            {
                                Title = $"Order #{o.Id} - {(o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : "Customer")}",
                                Description = $"Date: {o.OrderDate:dd.MM.yyyy} | Total: {o.TotalAmount:N2} EUR",
                                Category = "Orders",
                                Url = $"/orders/{o.Id}",
                                Icon = "bi-cart-check"
                            })
                            .ToListAsync();
                        results.AddRange(orders);
                    }

                    if (isAdmin)
                    {
                        var technicians = await _dbContext.Technicians
                            .Where(t => !t.IsDeleted && (
                                (t.FirstName + " " + t.LastName).ToLower().Contains(query) ||
                                t.Specialization.ToLower().Contains(query)
                            ))
                            .Take(5)
                            .Select(t => new SearchResultDto
                            {
                                Title = $"{t.FirstName} {t.LastName}",
                                Description = $"Specialization: {t.Specialization} | Active",
                                Category = "Technicians",
                                Url = $"/technicians/{t.Id}",
                                Icon = "bi-person-badge"
                            })
                            .ToListAsync();
                        results.AddRange(technicians);
                    }
                }
            }
            else
            {
                results.AddRange(menuItems.Take(4));
            }

            return Ok(results);
        }

        private List<SearchResultDto> GetAvailableMenuItems(bool isAdmin, bool isWorker, bool isCustomer, bool isAuthenticated)
        {
            var menuItems = new List<SearchResultDto>();

            menuItems.Add(new SearchResultDto { Title = "Home Page", Description = "Go back to the homepage and dashboard", Category = "Navigation", Url = "/", Icon = "bi-house" });
            menuItems.Add(new SearchResultDto { Title = "Store / Products", Description = "Browse available retail products and spare parts", Category = "Navigation", Url = "/products", Icon = "bi-shop" });
            menuItems.Add(new SearchResultDto { Title = "Track Repair", Description = "Quickly look up a repair status by ID", Category = "Navigation", Url = "/repair-jobs/tracker", Icon = "bi-search" });

            if (isCustomer || isAdmin)
            {
                menuItems.Add(new SearchResultDto { Title = "Orders", Description = "View purchase order logs and history", Category = "Navigation", Url = "/orders", Icon = "bi-cart" });
                menuItems.Add(new SearchResultDto { Title = "New Order", Description = "Create a new product sales order", Category = "Navigation", Url = "/orders/create", Icon = "bi-cart-plus" });
            }

            if (isWorker || isAdmin)
            {
                menuItems.Add(new SearchResultDto { Title = "Register Phone", Description = "Add a customer's phone to the database", Category = "Navigation", Url = "/phones/create", Icon = "bi-phone-plus" });
                menuItems.Add(new SearchResultDto { Title = "New Product", Description = "Add a new retail product to the catalog", Category = "Navigation", Url = "/products/create", Icon = "bi-plus-circle" });
                menuItems.Add(new SearchResultDto { Title = "Spare Parts", Description = "Manage replacement spare parts inventory", Category = "Navigation", Url = "/spare-parts", Icon = "bi-gear-wide-connected" });
                menuItems.Add(new SearchResultDto { Title = "New Spare Part", Description = "Add replacement parts to the inventory", Category = "Navigation", Url = "/spare-parts/create", Icon = "bi-plus-square" });
                menuItems.Add(new SearchResultDto { Title = "New Repair Job", Description = "Open a new repair job ticket", Category = "Navigation", Url = "/repair-jobs/create", Icon = "bi-file-earmark-plus" });
            }

            if (isAdmin || isWorker || isCustomer)
            {
                menuItems.Add(new SearchResultDto { Title = "Repair Jobs (Repairs)", Description = "Manage repair jobs and statuses", Category = "Navigation", Url = "/repair-jobs", Icon = "bi-tools" });
            }

            if (isAdmin)
            {
                menuItems.Add(new SearchResultDto { Title = "Customers", Description = "Manage registered customer database", Category = "Navigation", Url = "/customers", Icon = "bi-people" });
                menuItems.Add(new SearchResultDto { Title = "Phones", Description = "List of all registered devices in system", Category = "Navigation", Url = "/phones", Icon = "bi-phone" });
                menuItems.Add(new SearchResultDto { Title = "Technicians / Staff", Description = "Manage service staff and technicians", Category = "Navigation", Url = "/technicians", Icon = "bi-person-badge" });
                menuItems.Add(new SearchResultDto { Title = "New Technician", Description = "Add a new repair staff member", Category = "Navigation", Url = "/technicians/create", Icon = "bi-person-plus-fill" });
            }

            return menuItems;
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
    }
}
