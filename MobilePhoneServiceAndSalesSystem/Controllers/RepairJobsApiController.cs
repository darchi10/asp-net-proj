using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [ApiController]
    [Route("api/repair-jobs")]
    [Authorize(Roles = "Admin")]
    public class RepairJobsApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public RepairJobsApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RepairJobListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(rj =>
                    rj.Description.Contains(term)
                    || (rj.Technician != null
                        && (rj.Technician.FirstName + " " + rj.Technician.LastName).Contains(term)));
            }

            var repairJobs = query
                .Include(rj => rj.Phone)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .OrderByDescending(rj => rj.ReceivedDate)
                .ToList()
                .Select(rj => rj.ToListDto())
                .ToList();

            return Ok(repairJobs);
        }

        [HttpGet("{id:int}")]
        public ActionResult<RepairJobDetailsDto> Get(int id)
        {
            var repairJob = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.Phone)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            return Ok(repairJob.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<RepairJobDetailsDto> Post([FromBody] RepairJobDto dto)
        {
            if (!PhoneExists(dto.PhoneId))
            {
                return BadRequest("Phone does not exist.");
            }

            if (!TechnicianExists(dto.TechnicianId))
            {
                return BadRequest("Technician does not exist.");
            }

            var usedParts = GetUsedParts(dto.UsedPartIds);

            var repairJob = new RepairJob();
            repairJob.ApplyDto(dto);
            repairJob.UsedParts = usedParts;

            _dbContext.RepairJobs.Add(repairJob);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = repairJob.Id }, repairJob.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<RepairJobDetailsDto> Put(int id, [FromBody] RepairJobDto dto)
        {
            if (!PhoneExists(dto.PhoneId))
            {
                return BadRequest("Phone does not exist.");
            }

            if (!TechnicianExists(dto.TechnicianId))
            {
                return BadRequest("Technician does not exist.");
            }

            var repairJob = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.Phone)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .FirstOrDefault(rj => rj.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            var usedParts = GetUsedParts(dto.UsedPartIds);

            repairJob.ApplyDto(dto);
            repairJob.UsedParts = usedParts;
            _dbContext.SaveChanges();

            return Ok(repairJob.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var repairJob = _dbContext.RepairJobs
                .FirstOrDefault(rj => rj.Id == id && !rj.IsDeleted);

            if (repairJob is null)
            {
                return NotFound();
            }

            repairJob.IsDeleted = true;
            repairJob.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }

        private bool PhoneExists(int phoneId)
        {
            return _dbContext.Phones.Any(p => p.Id == phoneId && !p.IsDeleted);
        }

        private bool TechnicianExists(int technicianId)
        {
            return _dbContext.Technicians.Any(t => t.Id == technicianId && !t.IsDeleted);
        }

        private List<SparePart> GetUsedParts(IEnumerable<int>? usedPartIds)
        {
            var ids = usedPartIds?.Distinct().ToList() ?? new List<int>();
            if (!ids.Any())
            {
                return new List<SparePart>();
            }

            return _dbContext.SpareParts
                .Where(sp => ids.Contains(sp.Id) && !sp.IsDeleted)
                .ToList();
        }
    }
}
