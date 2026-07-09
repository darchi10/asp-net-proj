using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Infrastructure.AI;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("ai")]
    [Authorize(Roles = "Admin,Worker")]
    public class AiController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly GroqAiService _aiService;

        public AiController(AppDbContext dbContext, GroqAiService aiService)
        {
            _dbContext = dbContext;
            _aiService = aiService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("parse")]
        public async Task<IActionResult> Parse([FromBody] AiEntityParseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
            {
                return BadRequest(new { error = "Input cannot be empty." });
            }

            var entityType = NormalizeEntityType(request.EntityType);
            if (entityType == null)
            {
                return BadRequest(new { error = "Choose Product or Phone before generating a preview." });
            }

            if (entityType == AiEntityTypes.Product)
            {
                return await ParseProduct(request.Input);
            }

            return await ParsePhone(request.Input);
        }

        [HttpPost]
        [Route("confirm")]
        public IActionResult Confirm([FromBody] AiConfirmRequest request)
        {
            var entityType = NormalizeEntityType(request.EntityType);
            if (entityType == null)
            {
                return BadRequest(new { error = "Unknown entity type." });
            }

            if (entityType == AiEntityTypes.Product)
            {
                return ConfirmProduct(request.Product);
            }

            return ConfirmPhone(request.Phone);
        }

        private async Task<IActionResult> ParseProduct(string input)
        {
            const string systemPrompt = @"You are a product data parser. Extract product information from user input and return ONLY valid JSON with these exact fields:
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

            var product = await _aiService.ParseToEntityAsync<ProductDto>(input, systemPrompt);
            if (product == null)
            {
                return BadRequest(new { error = "Could not parse product input. Try being more specific." });
            }

            if (!TryValidatePayload(product, out var errors))
            {
                return BadRequest(new { error = "AI returned invalid product data.", errors });
            }

            return Ok(new AiParseResult
            {
                EntityType = AiEntityTypes.Product,
                Product = product
            });
        }

        private async Task<IActionResult> ParsePhone(string input)
        {
            const string systemPrompt = @"You are a phone data parser. Extract phone information from user input and return ONLY valid JSON with these exact fields:
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

            var result = await _aiService.ParseToEntityAsync<PhoneAiDto>(input, systemPrompt);
            if (result == null)
            {
                return BadRequest(new { error = "Could not parse phone input. Try being more specific." });
            }

            var preview = BuildPhonePreview(result);
            if (!TryValidatePayload(preview, out var errors))
            {
                return BadRequest(new { error = "AI returned invalid phone data.", errors });
            }

            return Ok(new AiParseResult
            {
                EntityType = AiEntityTypes.Phone,
                Phone = preview
            });
        }

        private IActionResult ConfirmProduct(ProductDto? dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Product data is missing." });
            }

            if (!TryValidatePayload(dto, out var errors))
            {
                return BadRequest(new { error = "Product data is invalid.", errors });
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                CurrentPrice = dto.CurrentPrice,
                StockQuantity = dto.StockQuantity
            };

            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            return Ok(new { redirectUrl = Url.Content("~/products") });
        }

        private IActionResult ConfirmPhone(PhoneDto? dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Phone data is missing." });
            }

            if (!TryValidatePayload(dto, out var errors))
            {
                return BadRequest(new { error = "Phone data is invalid.", errors });
            }

            var customerExists = _dbContext.Customers.Any(c => !c.IsDeleted && c.Id == dto.CustomerId);
            if (!customerExists)
            {
                return BadRequest(new { error = "Select an existing customer before confirming." });
            }

            var phone = new Phone
            {
                Brand = dto.Brand,
                Model = dto.Model,
                IMEI = dto.IMEI,
                YearOfManufacture = dto.YearOfManufacture,
                OperatingSystem = dto.OperatingSystem,
                CustomerId = dto.CustomerId
            };

            _dbContext.Phones.Add(phone);
            _dbContext.SaveChanges();

            return Ok(new { redirectUrl = Url.Content("~/phones") });
        }

        private PhoneAiPreviewDto BuildPhonePreview(PhoneAiDto result)
        {
            int? customerId = null;
            string? customerText = null;

            if (result.CustomerId.HasValue && result.CustomerId.Value > 0)
            {
                var customerById = _dbContext.Customers
                    .FirstOrDefault(c => !c.IsDeleted && c.Id == result.CustomerId.Value);

                if (customerById != null)
                {
                    customerId = customerById.Id;
                    customerText = $"{customerById.FirstName} {customerById.LastName}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(result.CustomerName))
            {
                var customerByName = _dbContext.Customers
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefault(c => (c.FirstName + " " + c.LastName).Contains(result.CustomerName));

                if (customerByName != null)
                {
                    customerId = customerByName.Id;
                    customerText = $"{customerByName.FirstName} {customerByName.LastName}";
                }
            }

            return new PhoneAiPreviewDto
            {
                Brand = result.Brand,
                Model = result.Model,
                IMEI = result.Imei,
                YearOfManufacture = result.YearOfManufacture,
                OperatingSystem = result.OperatingSystem,
                CustomerId = customerId,
                CustomerText = customerText,
                CustomerSearchTerm = result.CustomerName,
                ExplicitId = result.CustomerId,
                RequiresCustomerSelection = !customerId.HasValue
            };
        }

        private bool TryValidatePayload(object payload, out Dictionary<string, string[]> errors)
        {
            ModelState.Clear();
            var isValid = TryValidateModel(payload);
            errors = ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            return isValid;
        }

        private static string? NormalizeEntityType(string? entityType)
        {
            if (string.Equals(entityType, AiEntityTypes.Product, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, "products", StringComparison.OrdinalIgnoreCase))
            {
                return AiEntityTypes.Product;
            }

            if (string.Equals(entityType, AiEntityTypes.Phone, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, "phones", StringComparison.OrdinalIgnoreCase))
            {
                return AiEntityTypes.Phone;
            }

            return null;
        }
    }
}
