using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("phones")]
    [Authorize(Roles = "Admin,Worker")]
    public class PhonesController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly Infrastructure.AI.GroqAiService _aiService;
        private readonly ILogger<PhonesController> _logger;

        public PhonesController(AppDbContext dbContext, Infrastructure.AI.GroqAiService aiService, ILogger<PhonesController> logger)
        {
            _dbContext = dbContext;
            _aiService = aiService;
            _logger = logger;
        }

        [Route("")]
        public IActionResult Index()
        {
            var phones = _dbContext.Phones
                .Where(p => !p.IsDeleted)
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .ToList();

            return View(phones);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
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

            return View(phone);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var phones = _dbContext.Phones
                .Where(p => !p.IsDeleted
                    && (p.Brand.Contains(query) || p.Model.Contains(query) || (p.Customer != null && (p.Customer.FirstName + " " + p.Customer.LastName).Contains(query))))
                .Include(p => p.Customer)
                .Include(p => p.RepairJobs)
                .ToList();

            return PartialView("_PhoneCards", phones);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            PopulateCustomerSelection(null);
            return View();
        }

        [HttpPost]
        [Route("ai-parse")]
        public async Task<IActionResult> AiParse([FromBody] AiParseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
            {
                return BadRequest(new { error = "Input cannot be empty" });
            }

            var systemPrompt = @"You are a phone data parser. Extract phone information from user input and return ONLY valid JSON with these exact fields:
{
  ""brand"": ""phone brand (max 100 chars)"",
  ""model"": ""phone model (max 100 chars)"",
  ""imei"": ""IMEI number (15 digits)"",
  ""yearOfManufacture"": 2020,
  ""operatingSystem"": ""OS name (max 100 chars)"",
  ""customerName"": ""customer full name if mentioned"",
  ""customerId"": null or integer
}
Rules:
- yearOfManufacture must be between 1990 and 2100
- imei should be 15 digits, generate random if not provided
- customerName extract from context (e.g., 'phone belongs to John Doe', 'owner: Jane Smith')
- customerId extract if explicitly mentioned (e.g., 'customer ID 5', 'customerid: 123', 'ID #42')
- PRIORITY: If customerId is provided, use it over customerName
- Return ONLY the JSON object, no explanations";

            var result = await _aiService.ParseToEntityAsync<PhoneAiDto>(request.Input, systemPrompt);
            
            if (result == null)
            {
                return BadRequest(new { error = "Could not parse input. Try being more specific." });
            }

            // Prioritet: Ako je CustomerId eksplicitno naveden, koristi ga
            int? customerId = null;
            string? customerText = null;
            
            if (result.CustomerId.HasValue && result.CustomerId.Value > 0)
            {
                // ID eksplicitno naveden - provjeri postoji li
                var customerById = _dbContext.Customers
                    .Where(c => !c.IsDeleted && c.Id == result.CustomerId.Value)
                    .FirstOrDefault();
                
                if (customerById != null)
                {
                    customerId = customerById.Id;
                    customerText = $"{customerById.FirstName} {customerById.LastName}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(result.CustomerName))
            {
                // ID nije naveden, traži po imenu
                var customerByName = _dbContext.Customers
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefault(c => (c.FirstName + " " + c.LastName).Contains(result.CustomerName));
                
                if (customerByName != null)
                {
                    customerId = customerByName.Id;
                    customerText = $"{customerByName.FirstName} {customerByName.LastName}";
                }
            }

            return Ok(new
            {
                result.Brand,
                result.Model,
                result.Imei,
                result.YearOfManufacture,
                result.OperatingSystem,
                CustomerId = customerId,
                CustomerText = customerText,
                CustomerSearchTerm = result.CustomerName,
                ExplicitId = result.CustomerId
            });
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Phone phone)
        {
            if (!ModelState.IsValid)
            {
                PopulateCustomerSelection(phone.CustomerId);
                return View(phone);
            }

            _dbContext.Phones.Add(phone);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var phone = _dbContext.Phones.FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (phone is null)
            {
                return NotFound();
            }

            PopulateCustomerSelection(phone.CustomerId);
            return View(phone);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Phone phone)
        {
            if (id != phone.Id)
            {
                return NotFound();
            }

            var existingPhone = _dbContext.Phones.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (existingPhone is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PopulateCustomerSelection(phone.CustomerId);
                return View(phone);
            }

            existingPhone.Brand = phone.Brand;
            existingPhone.Model = phone.Model;
            existingPhone.IMEI = phone.IMEI;
            existingPhone.YearOfManufacture = phone.YearOfManufacture;
            existingPhone.OperatingSystem = phone.OperatingSystem;
            existingPhone.CustomerId = phone.CustomerId;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateCustomerSelection(int? selectedCustomerId)
        {
            ViewBag.SelectedCustomerText = string.Empty;
            if (!selectedCustomerId.HasValue || selectedCustomerId.Value <= 0)
            {
                return;
            }

            var label = _dbContext.Customers
                .Where(c => !c.IsDeleted && c.Id == selectedCustomerId.Value)
                .Select(c => c.FirstName + " " + c.LastName)
                .FirstOrDefault();

            ViewBag.SelectedCustomerText = label ?? string.Empty;
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
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

            ViewBag.HasDependencies = phone.RepairJobs.Any();
            return View(phone);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id, string deleteMode)
        {
            var phone = _dbContext.Phones
                .Include(p => p.RepairJobs)
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted);

            if (phone is null)
            {
                return NotFound();
            }

            var hasDependencies = phone.RepairJobs.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                phone.IsDeleted = true;
                phone.DeletedAt = System.DateTime.UtcNow;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            if (phone.RepairJobs.Any())
            {
                _dbContext.RepairJobs.RemoveRange(phone.RepairJobs);
            }

            _dbContext.Phones.Remove(phone);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}