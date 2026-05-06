using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("phones")]
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
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .ToList();

            return View(phones);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var phone = _dbContext.Phones
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
        [Route("create")]
        public IActionResult Create()
        {
            PopulateCustomers();
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Phone phone)
        {
            if (!ModelState.IsValid)
            {
                PopulateCustomers(phone.CustomerId);
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
            var phone = _dbContext.Phones.Find(id);

            if (phone is null)
            {
                return NotFound();
            }

            PopulateCustomers(phone.CustomerId);
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

            if (!ModelState.IsValid)
            {
                PopulateCustomers(phone.CustomerId);
                return View(phone);
            }

            _dbContext.Phones.Update(phone);
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
    }
}