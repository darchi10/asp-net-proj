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
    [Route("api/spare-parts")]
    [Authorize(Roles = "Admin")]
    public class SparePartsApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public SparePartsApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SparePartListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(sp =>
                    sp.Name.Contains(term)
                    || sp.Manufacturer.Contains(term));
            }

            var spareParts = query
                .Include(sp => sp.RepairJobs)
                .OrderBy(sp => sp.Name)
                .ThenBy(sp => sp.Manufacturer)
                .ToList()
                .Select(sp => sp.ToListDto())
                .ToList();

            return Ok(spareParts);
        }

        [HttpGet("{id:int}")]
        public ActionResult<SparePartDetailsDto> Get(int id)
        {
            var sparePart = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted)
                .Include(sp => sp.RepairJobs)
                .FirstOrDefault(sp => sp.Id == id);

            if (sparePart is null)
            {
                return NotFound();
            }

            return Ok(sparePart.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<SparePartDetailsDto> Post([FromBody] SparePartDto dto)
        {
            var sparePart = new SparePart();
            sparePart.ApplyDto(dto);

            _dbContext.SpareParts.Add(sparePart);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = sparePart.Id }, sparePart.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<SparePartDetailsDto> Put(int id, [FromBody] SparePartDto dto)
        {
            var sparePart = _dbContext.SpareParts
                .Where(sp => !sp.IsDeleted)
                .Include(sp => sp.RepairJobs)
                .FirstOrDefault(sp => sp.Id == id);

            if (sparePart is null)
            {
                return NotFound();
            }

            sparePart.ApplyDto(dto);
            _dbContext.SaveChanges();

            return Ok(sparePart.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var sparePart = _dbContext.SpareParts
                .FirstOrDefault(sp => sp.Id == id && !sp.IsDeleted);

            if (sparePart is null)
            {
                return NotFound();
            }

            sparePart.IsDeleted = true;
            sparePart.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
