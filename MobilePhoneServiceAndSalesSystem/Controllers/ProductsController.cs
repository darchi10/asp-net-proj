using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly Infrastructure.AI.GroqAiService _aiService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext dbContext, Infrastructure.AI.GroqAiService aiService, ILogger<ProductsController> logger)
        {
            _dbContext = dbContext;
            _aiService = aiService;
            _logger = logger;
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
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("ai-parse")]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<IActionResult> AiParse([FromBody] AiParseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
            {
                return BadRequest(new { error = "Input cannot be empty" });
            }

            var systemPrompt = @"You are a product data parser. Extract product information from user input and return ONLY valid JSON with these exact fields:
{
  ""name"": ""product name (max 150 chars)"",
  ""description"": ""product description (max 1000 chars)"",
  ""currentPrice"": 0.00,
  ""stockQuantity"": 0
}
Rules:
- currentPrice must be between 0.01 and 100000
- stockQuantity must be between 0 and 100000
- Return ONLY the JSON object, no explanations";

            var result = await _aiService.ParseToEntityAsync<ProductDto>(request.Input, systemPrompt);
            
            if (result == null)
            {
                return BadRequest(new { error = "Could not parse input. Try being more specific." });
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin,Worker")]
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
        [Authorize(Roles = "Admin,Worker")]
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
        [Authorize(Roles = "Admin,Worker")]
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