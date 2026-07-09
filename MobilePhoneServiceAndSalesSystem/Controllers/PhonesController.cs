using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("phones")]
    [Authorize(Roles = "Admin,Worker")]
    public class PhonesController : Controller
    {
        private readonly AppDbContext _dbContext;

        public PhonesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var phones = _dbContext.Phones
                .Where(p => !p.IsDeleted)
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .ToList();

            return View(phones);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var phone = _dbContext.Phones
                .Where(p => !p.IsDeleted)
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .FirstOrDefault(p => p.Id == id);

            if (phone is null)
            {
                return NotFound();
            }

            return View(phone);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var phones = _dbContext.Phones
                .Where(p => !p.IsDeleted
                    && (p.Brand.Contains(query) || p.Model.Contains(query) || (p.Customer != null && (p.Customer.FirstName + " " + p.Customer.LastName).Contains(query))))
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .ToList();

            return PartialView("_PhoneCards", phones);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            PopulateCustomerSelection(null);
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Phone phone)
        {
            if (!ModelState.IsValid)
            {
                PopulateCustomerSelection(phone.CustomerId);
                return View(phone);
            }

            _dbContext.Phones.Add(phone);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var phone = _dbContext.Phones.FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (phone is null)
            {
                return NotFound();
            }

            PopulateCustomerSelection(phone.CustomerId);
            return View(phone);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Phone phone)
        {
            if (id != phone.Id)
            {
                return NotFound();
            }

            var existingPhone = _dbContext.Phones.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (existingPhone is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PopulateCustomerSelection(phone.CustomerId);
                return View(phone);
            }

            existingPhone.Brand = phone.Brand;
            existingPhone.Model = phone.Model;
            existingPhone.IMEI = phone.IMEI;
            existingPhone.YearOfManufacture = phone.YearOfManufacture;
            existingPhone.OperatingSystem = phone.OperatingSystem;
            existingPhone.CustomerId = phone.CustomerId;
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

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var phone = _dbContext.Phones
                .Where(p => !p.IsDeleted)
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .FirstOrDefault(p => p.Id == id);

            if (phone is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = phone.RepairJobs.Any();
            return View(phone);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var phone = _dbContext.Phones
                .Include(p => p.RepairJobs)
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (phone is null)
            {
                return NotFound();
            }

            var hasDependencies = phone.RepairJobs.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                phone.IsDeleted = true;
                phone.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (phone.RepairJobs.Any())
            {
                _dbContext.RepairJobs.RemoveRange(phone.RepairJobs);
            }

            _dbContext.Phones.Remove(phone);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
