using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("spare-parts")]
    [Authorize(Roles = "Admin,Worker")]
    public class SparePartsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public SparePartsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var spareParts = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted)
                .Include(sp => sp.RepairJobs)
                .ToList();

            return View(spareParts);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var sparePart = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted)
                .Include(sp => sp.RepairJobs)
                .FirstOrDefault(sp => sp.Id == id);

            if (sparePart is null)
            {
                return NotFound();
            }

            return View(sparePart);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var spareParts = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted
                    && (sp.Name + " " + sp.Manufacturer).Contains(query))
                .Include(sp => sp.RepairJobs)
                .ToList();

            return PartialView("_SparePartCards", spareParts);
        }

        [HttpGet]
        [Route("create")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Create(SparePart sparePart)
        {
            if (!ModelState.IsValid)
            {
                return View(sparePart);
            }

            _dbContext.SpareParts.Add(sparePart);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Edit(int id)
        {
            var sparePart = _dbContext.SpareParts.FirstOrDefault(sp => sp.Id == id && !sp.IsDeleted);

            if (sparePart is null)
            {
                return NotFound();
            }

            return View(sparePart);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Edit(int id, SparePart sparePart)
        {
            if (id != sparePart.Id)
            {
                return NotFound();
            }

            var existingPart = _dbContext.SpareParts.FirstOrDefault(sp => sp.Id == id && !sp.IsDeleted);
            if (existingPart is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(sparePart);
            }

            existingPart.Name = sparePart.Name;
            existingPart.Manufacturer = sparePart.Manufacturer;
            existingPart.Price = sparePart.Price;
            existingPart.StockQuantity = sparePart.StockQuantity;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var sparePart = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted)
                .Include(sp => sp.RepairJobs)
                .FirstOrDefault(sp => sp.Id == id);

            if (sparePart is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = sparePart.RepairJobs.Any();
            return View(sparePart);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var sparePart = _dbContext.SpareParts
                .Include(sp => sp.RepairJobs)
                .FirstOrDefault(sp => sp.Id == id && !sp.IsDeleted);

            if (sparePart is null)
            {
                return NotFound();
            }

            var hasDependencies = sparePart.RepairJobs.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                sparePart.IsDeleted = true;
                sparePart.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (sparePart.RepairJobs.Any())
            {
                _dbContext.RepairJobs.RemoveRange(sparePart.RepairJobs);
            }

            _dbContext.SpareParts.Remove(sparePart);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}