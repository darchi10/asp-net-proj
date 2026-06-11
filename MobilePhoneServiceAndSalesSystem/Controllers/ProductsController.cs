using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public ProductsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = _dbContext.Products
                .Where(p => !p.IsDeleted)
                .ToList();

            return View(products);
        }

        [Route("{id:int}")]
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var product = _dbContext.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.OrderItems)
                .FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        [Route("search-list")]
        [AllowAnonymous]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var products = _dbContext.Products
                .Where(p => !p.IsDeleted
                    && (p.Name + " " + p.Description).Contains(query))
                .ToList();

            return PartialView("_ProductCards", products);
        }

        [HttpGet]
        [Route("create")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var existingProduct = _dbContext.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (existingProduct is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.CurrentPrice = product.CurrentPrice;
            existingProduct.StockQuantity = product.StockQuantity;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.OrderItems)
                .FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = product.OrderItems.Any();
            return View(product);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var product = _dbContext.Products
                .Include(p => p.OrderItems)
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (product is null)
            {
                return NotFound();
            }

            var hasDependencies = product.OrderItems.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Products with related orders can only be soft deleted.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase) || hasDependencies)
            {
                product.IsDeleted = true;
                product.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (product.OrderItems.Any())
            {
                _dbContext.OrderItems.RemoveRange(product.OrderItems);
            }

            _dbContext.Products.Remove(product);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}