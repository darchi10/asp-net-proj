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
    [Route("api/technicians")]
    [Authorize(Roles = "Admin")]
    public class TechniciansApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TechniciansApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TechnicianListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.Technicians
                .Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(t =>
                    (t.FirstName + " " + t.LastName).Contains(term)
                    || t.Specialization.Contains(term));
            }

            var technicians = query
                .Include(t => t.RepairJobs)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToList()
                .Select(t => t.ToListDto())
                .ToList();

            return Ok(technicians);
        }

        [HttpGet("{id:int}")]
        public ActionResult<TechnicianDetailsDto> Get(int id)
        {
            var technician = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            return Ok(technician.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<TechnicianDetailsDto> Post([FromBody] TechnicianDto dto)
        {
            var technician = new Technician();
            technician.ApplyDto(dto);

            _dbContext.Technicians.Add(technician);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = technician.Id }, technician.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<TechnicianDetailsDto> Put(int id, [FromBody] TechnicianDto dto)
        {
            var technician = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            technician.ApplyDto(dto);
            _dbContext.SaveChanges();

            return Ok(technician.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var technician = _dbContext.Technicians
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);

            if (technician is null)
            {
                return NotFound();
            }

            technician.IsDeleted = true;
            technician.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
