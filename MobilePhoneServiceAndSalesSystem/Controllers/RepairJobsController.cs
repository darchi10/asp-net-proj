using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("repair-jobs")]
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
            var repairJobs = _dbContext.RepairJobs
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .ToList();

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
                    .Include(rj => rj.Phone)
                    .Include(rj => rj.Technician)
                    .FirstOrDefault(rj => rj.Id == searchId.Value);
                if (repairJob != null)
                {
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
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            return View(repairJob);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            PopulateLookups();
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(RepairJob repairJob, int[] usedPartIds)
        {
            if (!ModelState.IsValid)
            {
                PopulateLookups(repairJob.PhoneId, repairJob.TechnicianId, usedPartIds);
                return View(repairJob);
            }

            repairJob.UsedParts = GetUsedParts(usedPartIds);

            _dbContext.RepairJobs.Add(repairJob);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var repairJob = _dbContext.RepairJobs
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            PopulateLookups(repairJob.PhoneId, repairJob.TechnicianId, repairJob.UsedParts.Select(p => p.Id));
            return View(repairJob);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, RepairJob repairJob, int[] usedPartIds)
        {
            if (id != repairJob.Id)
            {
                return NotFound();
            }

            var existingJob = _dbContext.RepairJobs
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (existingJob is null)
            {
                return NotFound();
            }

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
            existingJob.UsedParts = GetUsedParts(usedPartIds);

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateLookups(int? phoneId = null, int? technicianId = null, IEnumerable<int>? usedPartIds = null)
        {
            var phones = _dbContext.Phones
                .OrderBy(p => p.Brand)
                .ThenBy(p => p.Model)
                .Select(p => new { p.Id, Label = p.Brand + " " + p.Model + " (" + p.IMEI + ")" })
                .ToList();

            var technicians = _dbContext.Technicians
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
                .ToList();

            var spareParts = _dbContext.SpareParts
                .OrderBy(sp => sp.Name)
                .Select(sp => new { sp.Id, Label = sp.Name + " (" + sp.Manufacturer + ")" })
                .ToList();

            ViewBag.PhoneId = new SelectList(phones, "Id", "Label", phoneId);
            ViewBag.TechnicianId = new SelectList(technicians, "Id", "Name", technicianId);
            ViewBag.UsedPartIds = new MultiSelectList(spareParts, "Id", "Label", usedPartIds);
        }

        private List<SparePart> GetUsedParts(IEnumerable<int>? usedPartIds)
        {
            var ids = usedPartIds?.Distinct().ToList() ?? new List<int>();
            if (!ids.Any())
            {
                return new List<SparePart>();
            }

            return _dbContext.SpareParts.Where(sp => ids.Contains(sp.Id)).ToList();
        }
    }
}