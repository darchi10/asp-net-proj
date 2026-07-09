using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("repair-jobs")]
    [Authorize(Roles = "Admin,Worker,Customer")]
    public class RepairJobsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public RepairJobsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var repairJobsQuery = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .Include(rj => rj.Phone)
                .ThenInclude(p => p.Customer)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admin sees everything, no filtering needed
            }
            else if (User.IsInRole("Worker"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Forbid();
                }

                repairJobsQuery = repairJobsQuery.Where(rj => rj.Technician != null && rj.Technician.UserId == userId);
            }
            else if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue)
                {
                    return Forbid();
                }

                repairJobsQuery = repairJobsQuery.Where(rj => rj.Phone != null
                    && rj.Phone.CustomerId == customerId.Value);
            }

            var repairJobs = repairJobsQuery.ToList();

            return View(repairJobs);
        }

        [HttpGet]
        [Route("tracker/{searchId:int?}")]
        [Route("/RepairJobs/Tracker/{searchId:int?}")]
        public IActionResult Tracker(int? searchId)
        {
            if (searchId.HasValue)
            {
                var repairJob = _dbContext.RepairJobs
                    .Where(rj => !rj.IsDeleted)
                    .Include(rj => rj.Phone)
                    .ThenInclude(p => p.Customer)
                    .Include(rj => rj.Technician)
                    .FirstOrDefault(rj => rj.Id == searchId.Value);
                if (repairJob != null)
                {
                    if (User.IsInRole("Admin"))
                    {
                        // Admin sees everything
                    }
                    else if (User.IsInRole("Worker"))
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (string.IsNullOrWhiteSpace(userId) || repairJob.Technician?.UserId != userId)
                        {
                            ViewBag.ErrorTitle = "Access denied";
                            ViewBag.ErrorMessage = "You do not have access to this repair job.";
                            return View();
                        }
                    }
                    else if (User.IsInRole("Customer"))
                    {
                        var customerId = EnsureCustomerLink();
                        if (!customerId.HasValue || repairJob.Phone?.CustomerId != customerId.Value)
                        {
                            ViewBag.ErrorTitle = "Access denied";
                            ViewBag.ErrorMessage = "You do not have access to this repair job.";
                            return View();
                        }
                    }

                    ViewBag.RepairJob = repairJob;
                }
                else
                {
                    ViewBag.ErrorMessage = $"No repair job found with ID {searchId.Value}.";
                }
            }
            return View();
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var repairJob = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.Phone)
                .ThenInclude(p => p.Customer)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                // Admin sees everything
            }
            else if (User.IsInRole("Worker"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId) || repairJob.Technician?.UserId != userId)
                {
                    return Forbid();
                }
            }
            else if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue || repairJob.Phone?.CustomerId != customerId.Value)
                {
                    return Forbid();
                }
            }

            return View(repairJob);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var repairJobsQuery = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted
                    && (rj.Description.Contains(query) 
                        || (rj.Technician != null && (rj.Technician.FirstName + " " + rj.Technician.LastName).Contains(query))))
                .Include(rj => rj.Phone)
                .ThenInclude(p => p.Customer)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admin sees everything
            }
            else if (User.IsInRole("Worker"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Forbid();
                }

                repairJobsQuery = repairJobsQuery.Where(rj => rj.Technician != null && rj.Technician.UserId == userId);
            }
            else if (User.IsInRole("Customer"))
            {
                var customerId = EnsureCustomerLink();
                if (!customerId.HasValue)
                {
                    return Forbid();
                }

                repairJobsQuery = repairJobsQuery.Where(rj => rj.Phone != null
                    && rj.Phone.CustomerId == customerId.Value);
            }

            var repairJobs = repairJobsQuery.ToList();

            return PartialView("_RepairJobCards", repairJobs);
        }

        [HttpGet]
        [Route("create")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Create()
        {
            PopulateLookups();
            return View();
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Create(RepairJob repairJob, int[] usedPartIds)
        {
            AddLifecycleErrors(repairJob.Status, repairJob.ReceivedDate, repairJob.CompletedDate);
            TryGetValidatedReferences(
                repairJob.PhoneId,
                repairJob.TechnicianId,
                usedPartIds,
                out var usedParts);

            if (!ModelState.IsValid)
            {
                PopulateLookups(repairJob.PhoneId, repairJob.TechnicianId, usedPartIds);
                return View(repairJob);
            }

            repairJob.UsedParts = usedParts;

            _dbContext.RepairJobs.Add(repairJob);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Edit(int id)
        {
            var repairJob = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.UsedParts)
            .Include(rj => rj.Technician)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            if (User.IsInRole("Worker"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId) || repairJob.Technician?.UserId != userId)
                {
                    return Forbid();
                }
            }

            PopulateLookups(repairJob.PhoneId, repairJob.TechnicianId, repairJob.UsedParts.Select(p => p.Id));
            return View(repairJob);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Edit(int id, RepairJob repairJob, int[] usedPartIds)
        {
            if (id != repairJob.Id)
            {
                return NotFound();
            }

            var existingJob = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.UsedParts)
                .Include(rj => rj.Technician)
                .FirstOrDefault(rj => rj.Id == id);

            if (existingJob is null)
            {
                return NotFound();
            }

            if (User.IsInRole("Worker"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId) || existingJob.Technician?.UserId != userId)
                {
                    return Forbid();
                }

                AddLifecycleErrors(
                    repairJob.Status,
                    existingJob.ReceivedDate,
                    repairJob.CompletedDate,
                    existingJob.Status);

                if (!ModelState.IsValid)
                {
                    repairJob.PhoneId = existingJob.PhoneId;
                    repairJob.TechnicianId = existingJob.TechnicianId;
                    repairJob.ReceivedDate = existingJob.ReceivedDate;
                    repairJob.UsedParts = existingJob.UsedParts;
                    PopulateLookups(existingJob.PhoneId, existingJob.TechnicianId, existingJob.UsedParts.Select(p => p.Id));
                    return View(repairJob);
                }

                existingJob.Status = repairJob.Status;
                existingJob.CompletedDate = repairJob.CompletedDate;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            AddLifecycleErrors(
                repairJob.Status,
                repairJob.ReceivedDate,
                repairJob.CompletedDate,
                existingJob.Status);
            TryGetValidatedReferences(
                repairJob.PhoneId,
                repairJob.TechnicianId,
                usedPartIds,
                out var usedParts);

            if (!ModelState.IsValid)
            {
                PopulateLookups(repairJob.PhoneId, repairJob.TechnicianId, usedPartIds);
                return View(repairJob);
            }

            existingJob.Description = repairJob.Description;
            existingJob.Status = repairJob.Status;
            existingJob.ReceivedDate = repairJob.ReceivedDate;
            existingJob.CompletedDate = repairJob.CompletedDate;
            existingJob.LaborCost = repairJob.LaborCost;
            existingJob.PhoneId = repairJob.PhoneId;
            existingJob.TechnicianId = repairJob.TechnicianId;
            existingJob.UsedParts = usedParts;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var repairJob = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = repairJob.UsedParts.Any();
            return View(repairJob);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var repairJob = _dbContext.RepairJobs
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id && !rj.IsDeleted);

            if (repairJob is null)
            {
                return NotFound();
            }

            var hasDependencies = repairJob.UsedParts.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                repairJob.IsDeleted = true;
                repairJob.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (repairJob.UsedParts.Any())
            {
                repairJob.UsedParts.Clear();
            }

            _dbContext.RepairJobs.Remove(repairJob);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateLookups(int? phoneId = null, int? technicianId = null, IEnumerable<int>? usedPartIds = null)
        {
            var phones = _dbContext.Phones
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Brand)
                .ThenBy(p => p.Model)
                .Select(p => new { p.Id, Label = p.Brand + " " + p.Model + " (" + p.IMEI + ")" })
                .ToList();

            var technicians = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
                .ToList();

            var spareParts = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted)
                .OrderBy(sp => sp.Name)
                .Select(sp => new { sp.Id, Label = sp.Name + " (" + sp.Manufacturer + ")" })
                .ToList();

            ViewBag.PhoneId = new SelectList(phones, "Id", "Label", phoneId);
            ViewBag.TechnicianId = new SelectList(technicians, "Id", "Name", technicianId);
            ViewBag.UsedPartIds = new MultiSelectList(spareParts, "Id", "Label", usedPartIds);
        }

        private void AddLifecycleErrors(
            MobilePhoneServiceAndSalesSystem.Models.Enums.RepairStatus status,
            DateTime receivedDate,
            DateTime? completedDate,
            MobilePhoneServiceAndSalesSystem.Models.Enums.RepairStatus? currentStatus = null)
        {
            var errors = currentStatus.HasValue
                ? RepairJobLifecycleRules.ValidateUpdate(currentStatus.Value, status, receivedDate, completedDate, DateTime.Now)
                : RepairJobLifecycleRules.ValidateSnapshot(status, receivedDate, completedDate, DateTime.Now);

            foreach (var error in errors)
            {
                ModelState.AddModelError(error.Key, error.Message);
            }
        }

        private bool TryGetValidatedReferences(
            int phoneId,
            int technicianId,
            IEnumerable<int>? usedPartIds,
            out List<SparePart> usedParts)
        {
            var isValid = true;

            if (!_dbContext.Phones.Any(p => p.Id == phoneId && !p.IsDeleted))
            {
                ModelState.AddModelError(nameof(RepairJob.PhoneId), "Select an active device.");
                isValid = false;
            }

            if (!_dbContext.Technicians.Any(t => t.Id == technicianId && !t.IsDeleted))
            {
                ModelState.AddModelError(nameof(RepairJob.TechnicianId), "Select an active technician.");
                isValid = false;
            }

            var ids = usedPartIds?.Distinct().ToList() ?? new List<int>();
            if (!ids.Any())
            {
                usedParts = new List<SparePart>();
                return isValid;
            }

            usedParts = _dbContext.SpareParts
                .Where(sp => ids.Contains(sp.Id) && !sp.IsDeleted)
                .ToList();

            if (usedParts.Count != ids.Count)
            {
                ModelState.AddModelError("usedPartIds", "One or more selected spare parts are unavailable.");
                isValid = false;
            }

            return isValid;
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
