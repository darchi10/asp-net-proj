using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("customers")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly AppDbContext _dbContext;

        public CustomersController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var customers = _dbContext.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Phones)
                .Include(c => c.Orders)
                .ToList();

            return View(customers);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var customer = _dbContext.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Phones)
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        [Route("search")]
        public IActionResult Search(string? term)
        {
            var query = term?.Trim() ?? string.Empty;
            if (query.Length < 2)
            {
                return Json(System.Array.Empty<object>());
            }

            var results = _dbContext.Customers
                .Where(c => !c.IsDeleted
                    && (c.FirstName + " " + c.LastName).Contains(query))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new
                {
                    id = c.Id,
                    text = c.FirstName + " " + c.LastName
                })
                .Take(10)
                .ToList();

            return Json(results);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var customers = _dbContext.Customers
                .Where(c => !c.IsDeleted
                    && (c.FirstName + " " + c.LastName).Contains(query))
                .Include(c => c.Phones)
                .Include(c => c.Orders)
                .ToList();

            return PartialView("_CustomerCards", customers);
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (customer is null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            var existingCustomer = _dbContext.Customers.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (existingCustomer is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;
            existingCustomer.Address = customer.Address;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var customer = _dbContext.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Phones)
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = customer.Orders.Any() || customer.Phones.Any();
            return View(customer);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var customer = _dbContext.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
                .Include(c => c.Phones)
                .ThenInclude(p => p.RepairJobs)
                .FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (customer is null)
            {
                return NotFound();
            }

            var hasDependencies = customer.Orders.Any() || customer.Phones.Any();
            if (hasDependencies && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for customers with related orders or devices.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                customer.IsDeleted = true;
                customer.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            var orderItems = customer.Orders.SelectMany(o => o.OrderItems).ToList();
            if (orderItems.Any())
            {
                _dbContext.OrderItems.RemoveRange(orderItems);
            }

            if (customer.Orders.Any())
            {
                _dbContext.Orders.RemoveRange(customer.Orders);
            }

            var repairJobs = customer.Phones.SelectMany(p => p.RepairJobs).ToList();
            if (repairJobs.Any())
            {
                _dbContext.RepairJobs.RemoveRange(repairJobs);
            }

            if (customer.Phones.Any())
            {
                _dbContext.Phones.RemoveRange(customer.Phones);
            }

            _dbContext.Customers.Remove(customer);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}