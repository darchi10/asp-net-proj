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
    [Route("api/products")]
    [Authorize(Roles = "Admin")]
    public class ProductsApiController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ProductsApiController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductListDto>> Get([FromQuery] string? q)
        {
            var query = _dbContext.Products
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term)
                    || p.Description.Contains(term));
            }

            var products = query
                .Include(p => p.OrderItems)
                .OrderBy(p => p.Name)
                .ToList()
                .Select(p => p.ToListDto())
                .ToList();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ProductDetailsDto> Get(int id)
        {
            var product = _dbContext.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.OrderItems)
                .FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product.ToDetailsDto());
        }

        [HttpPost]
        public ActionResult<ProductDetailsDto> Post([FromBody] ProductDto dto)
        {
            var product = new Product();
            product.ApplyDto(dto);

            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product.ToDetailsDto());
        }

        [HttpPut("{id:int}")]
        public ActionResult<ProductDetailsDto> Put(int id, [FromBody] ProductDto dto)
        {
            var product = _dbContext.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.OrderItems)
                .FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            product.ApplyDto(dto);
            _dbContext.SaveChanges();

            return Ok(product.ToDetailsDto());
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (product is null)
            {
                return NotFound();
            }

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
