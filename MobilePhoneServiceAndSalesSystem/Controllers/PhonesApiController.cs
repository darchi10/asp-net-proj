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
    [Route("api/phones")]
    [Authorize(Roles = "Admin")]
    public class PhonesApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public PhonesApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PhoneListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.Phones
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p =>
                    p.Brand.Contains(term)
                    || p.Model.Contains(term)
                    || p.IMEI.Contains(term)
                    || (p.Customer != null &&
                        (p.Customer.FirstName + " " + p.Customer.LastName).Contains(term)));
            }

            var phones = query
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .OrderBy(p => p.Brand)
                .ThenBy(p => p.Model)
                .ToList()
                .Select(p => p.ToListDto())
                .ToList();

            return Ok(phones);
        }

        [HttpGet("{id:int}")]
        public ActionResult<PhoneDetailsDto> Get(int id)
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

            return Ok(phone.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<PhoneDetailsDto> Post([FromBody] PhoneDto dto)
        {
            if (!CustomerExists(dto.CustomerId))
            {
                return BadRequest("Customer does not exist.");
            }

            var phone = new Phone();
            phone.ApplyDto(dto);

            _dbContext.Phones.Add(phone);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = phone.Id }, phone.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<PhoneDetailsDto> Put(int id, [FromBody] PhoneDto dto)
        {
            if (!CustomerExists(dto.CustomerId))
            {
                return BadRequest("Customer does not exist.");
            }

            var phone = _dbContext.Phones
                .Where(p => !p.IsDeleted)
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .FirstOrDefault(p => p.Id == id);

            if (phone is null)
            {
                return NotFound();
            }

            phone.ApplyDto(dto);
            _dbContext.SaveChanges();

            return Ok(phone.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var phone = _dbContext.Phones
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (phone is null)
            {
                return NotFound();
            }

            phone.IsDeleted = true;
            phone.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }

        private bool CustomerExists(int customerId)
        {
            return _dbContext.Customers.Any(c => c.Id == customerId && !c.IsDeleted);
        }
    }
}
