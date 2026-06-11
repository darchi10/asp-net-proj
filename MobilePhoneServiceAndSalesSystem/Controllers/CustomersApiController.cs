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
    [Route("api/customers")]
    [Authorize(Roles = "Admin")]
    public class CustomersApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public CustomersApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CustomerListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.Customers
                .Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    (c.FirstName + " " + c.LastName).Contains(term)
                    || c.Email.Contains(term));
            }

            var customers = query
                .Include(c => c.Orders)
                .Include(c => c.Phones)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList()
                .Select(c => c.ToListDto())
                .ToList();

            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public ActionResult<CustomerDetailsDto> Get(int id)
        {
            var customer = _dbContext.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Orders)
                .Include(c => c.Phones)
                .FirstOrDefault(c => c.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            return Ok(customer.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<CustomerDetailsDto> Post([FromBody] CustomerDto dto)
        {
            var customer = new Customer();
            customer.ApplyDto(dto);

            _dbContext.Customers.Add(customer);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<CustomerDetailsDto> Put(int id, [FromBody] CustomerDto dto)
        {
            var customer = _dbContext.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Orders)
                .Include(c => c.Phones)
                .FirstOrDefault(c => c.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            customer.ApplyDto(dto);
            _dbContext.SaveChanges();

            return Ok(customer.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var customer = _dbContext.Customers
                .FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (customer is null)
            {
                return NotFound();
            }

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
