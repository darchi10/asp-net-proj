using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("technicians")]
    public class TechniciansController : Controller
    {
        private readonly AppDbContext _dbContext;

        public TechniciansController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var technicians = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .ToList();

            return View(technicians);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var technician = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Technician technician)
        {
            if (!ModelState.IsValid)
            {
                return View(technician);
            }

            _dbContext.Technicians.Add(technician);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var technician = _dbContext.Technicians.FirstOrDefault(t => t.Id == id && !t.IsDeleted);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Technician technician)
        {
            if (id != technician.Id)
            {
                return NotFound();
            }

            var existingTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (existingTechnician is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(technician);
            }

            existingTechnician.FirstName = technician.FirstName;
            existingTechnician.LastName = technician.LastName;
            existingTechnician.Specialization = technician.Specialization;
            existingTechnician.HireDate = technician.HireDate;
            existingTechnician.Salary = technician.Salary;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var technician = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = technician.RepairJobs.Any();
            return View(technician);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var technician = _dbContext.Technicians
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);

            if (technician is null)
            {
                return NotFound();
            }

            var hasDependencies = technician.RepairJobs.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                technician.IsDeleted = true;
                technician.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (technician.RepairJobs.Any())
            {
                _dbContext.RepairJobs.RemoveRange(technician.RepairJobs);
            }

            _dbContext.Technicians.Remove(technician);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}