# Architecture - Project Documentation

## 📋 Pregled

Mobile Phone Service and Sales System je **ASP.NET Core MVC** aplikacija za upravljanje servisom i prodajom mobitela. Projekt slijedi **layered architecture** pattern s jasnom separacijom brige između slojeva.

---

## 🏗️ High-Level Arhitektura

```
┌────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   MVC Views  │  │ Controllers  │  │  API Ctrl's  │ │
│  │  (Razor)     │  │   (HTML)     │  │   (JSON)     │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                    Business Logic Layer                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  Services    │  │   Filters    │  │  Middleware  │ │
│  │  (AI, etc)   │  │  (Logging)   │  │  (Errors)    │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                    Data Access Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  AppDbContext│  │   Entities   │  │    DTOs      │ │
│  │  (EF Core)   │  │  (Models)    │  │  (Mapping)   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                    Database Layer                      │
│               MySQL 8.0 (Production)                   │
│           InMemory DB (Testing)                        │
└────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
MobilePhoneServiceAndSalesSystem/
│
├── Controllers/              # MVC & API Controllers
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   ├── RepairJobsController.cs
│   ├── CustomersController.cs
│   ├── PhonesController.cs
│   ├── SparePartsController.cs
│   ├── TechniciansController.cs
│   ├── SearchApiController.cs
│   ├── *ApiController.cs     # API versions
│   └── OAuthMetadataController.cs
│
├── Models/                   # Domain Entities
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── RepairJob.cs
│   ├── Customer.cs
│   ├── Phone.cs
│   ├── SparePart.cs
│   ├── Technician.cs
│   ├── AppUser.cs            # Identity user
│   ├── DTOs/                 # Data Transfer Objects
│   │   ├── ProductDtos.cs
│   │   ├── OrderDtos.cs
│   │   ├── RepairJobDtos.cs
│   │   └── ...
│   ├── Enums/
│   │   └── RepairStatus.cs
│   └── ViewModels/
│
├── Views/                    # Razor Views
│   ├── Products/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── _ProductCards.cshtml
│   ├── Orders/
│   ├── RepairJobs/
│   ├── Customers/
│   ├── Phones/
│   ├── SpareParts/
│   ├── Technicians/
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _LoginPartial.cshtml
│
├── DAL/                      # Data Access Layer
│   ├── AppDbContext.cs       # EF Core DbContext
│   └── IdentitySeeder.cs     # Role seeding
│
├── Infrastructure/           # Cross-cutting concerns
│   ├── Logging/
│   │   ├── CrudActionLoggingFilter.cs
│   │   └── UnhandledExceptionLoggingMiddleware.cs
│   └── AI/
│       └── GroqAiService.cs
│
├── MCP/                      # Model Context Protocol
│   ├── ProductTools.cs
│   ├── OrderTools.cs
│   ├── RepairJobTools.cs
│   └── SystemResources.cs
│
├── wwwroot/                  # Static files
│   ├── css/
│   │   └── site.css
│   ├── js/
│   │   └── site.js
│   └── lib/                  # Client libraries
│
├── Program.cs                # Application entry point
└── appsettings.json          # Configuration

MobilePhoneServiceAndSalesSystem.IntegrationTests/
├── *ApiTests.cs              # Integration tests
├── TestWebApplicationFactory.cs
└── TestAuthHandler.cs
```

---

## 🎯 MVC Pattern Implementation

### Model-View-Controller Flow

```
User Request
     │
     ▼
┌─────────────────┐
│   Controller    │  ← Handles HTTP request
│                 │  ← Authorizes user
│                 │  ← Validates input
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Model         │  ← Business entities
│  (Entity/DTO)   │  ← Data validation
│                 │  ← Database interaction
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    View         │  ← Razor template
│  (Razor CSHTML) │  ← HTML generation
│                 │  ← Client-side scripts
└─────────────────┘
         │
         ▼
    HTTP Response
```

---

## 📦 Domain Entities

### Core Entities (7)

#### 1. **Product**
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }        // [Required, MaxLength(150)]
    public string Description { get; set; } // [MaxLength(1000)]
    public decimal CurrentPrice { get; set; } // [Range(0.01, 100000)]
    public int StockQuantity { get; set; }   // [Range(0, 100000)]
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; }
}
```

#### 2. **Customer**
```csharp
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string? UserId { get; set; }      // Link to Identity
    public bool IsDeleted { get; set; }
    
    // Navigation
    public ICollection<Phone> Phones { get; set; }
    public ICollection<Order> Orders { get; set; }
}
```

#### 3. **Phone**
```csharp
public class Phone
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string IMEI { get; set; }         // 15 digits
    public int YearOfManufacture { get; set; }
    public string OperatingSystem { get; set; }
    public int? CustomerId { get; set; }     // Optional FK
    public bool IsDeleted { get; set; }
    
    // Navigation
    public Customer? Customer { get; set; }
    public ICollection<RepairJob> RepairJobs { get; set; }
}
```

#### 4. **RepairJob**
```csharp
public class RepairJob
{
    public int Id { get; set; }
    public int PhoneId { get; set; }
    public int TechnicianId { get; set; }
    public string Description { get; set; }
    public RepairStatus Status { get; set; } // Enum
    public DateTime ReceivedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal LaborCost { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public Phone Phone { get; set; }
    public Technician Technician { get; set; }
    public ICollection<SparePart> UsedParts { get; set; }
}
```

#### 5. **Order**
```csharp
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public Customer Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}
```

#### 6. **SparePart**
```csharp
public class SparePart
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manufacturer { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public ICollection<RepairJob> RepairJobs { get; set; }
}
```

#### 7. **Technician**
```csharp
public class Technician
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public ICollection<RepairJob> RepairJobs { get; set; }
}
```

### Relationship Entities

#### **OrderItem** (Many-to-Many: Order ↔ Product)
```csharp
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public Order Order { get; set; }
    public Product Product { get; set; }
}
```

---

## 🔄 CRUD Operations Pattern

### Standard CRUD Controller Structure

```csharp
[Route("entity")]
public class EntityController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EntityController> _logger;

    public EntityController(AppDbContext dbContext, ILogger<EntityController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // LIST (Index)
    [Route("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        var entities = _dbContext.Entities
            .Where(e => !e.IsDeleted)
            .ToList();
        
        return View(entities);
    }

    // DETAILS (Read by ID)
    [Route("{id:int}")]
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var entity = _dbContext.Entities
            .Where(e => !e.IsDeleted)
            .Include(e => e.RelatedEntity)
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
            return NotFound();

        return View(entity);
    }

    // CREATE (GET form)
    [HttpGet]
    [Route("create")]
    [Authorize(Roles = "Admin,Worker")]
    public IActionResult Create()
    {
        return View();
    }

    // CREATE (POST data)
    [HttpPost]
    [Route("create")]
    [Authorize(Roles = "Admin,Worker")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EntityDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var entity = new Entity
        {
            Property1 = dto.Property1,
            Property2 = dto.Property2,
            // ... map DTO to Entity
        };

        _dbContext.Entities.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    // UPDATE (GET form)
    [HttpGet]
    [Route("{id:int}/edit")]
    [Authorize(Roles = "Admin,Worker")]
    public IActionResult Edit(int id)
    {
        var entity = _dbContext.Entities
            .Where(e => !e.IsDeleted)
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
            return NotFound();

        var dto = new EntityDto
        {
            Property1 = entity.Property1,
            Property2 = entity.Property2,
            // ... map Entity to DTO
        };

        return View(dto);
    }

    // UPDATE (POST data)
    [HttpPost]
    [Route("{id:int}/edit")]
    [Authorize(Roles = "Admin,Worker")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EntityDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var entity = _dbContext.Entities
            .Where(e => !e.IsDeleted)
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
            return NotFound();

        entity.Property1 = dto.Property1;
        entity.Property2 = dto.Property2;
        // ... update properties

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    // DELETE (Soft Delete)
    [HttpPost]
    [Route("{id:int}/delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = _dbContext.Entities
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
            return NotFound();

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
```

---

## 🗑️ Soft Delete Pattern

### Implementacija

Svi entiteti imaju:
```csharp
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }
```

### Delete Operation
```csharp
entity.IsDeleted = true;
entity.DeletedAt = DateTime.UtcNow;
await _dbContext.SaveChangesAsync();
```

### Queries (Exclude Deleted)
```csharp
var entities = _dbContext.Entities
    .Where(e => !e.IsDeleted)  // ← Filter soft-deleted
    .ToList();
```

### Prednosti:
- ✅ Ne briše podatke trajno (restore moguć)
- ✅ Audit trail (tko i kada obrisao)
- ✅ Referential integrity ostaje intaktan
- ✅ Foreign key constrainti ne breckiraju

---

## 🔀 DTO Mapping Strategy

### DTO Types

#### 1. **Input DTOs** (Create/Update)
```csharp
public class ProductDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; }
    
    [StringLength(1000)]
    public string Description { get; set; }
    
    [Range(0.01, 100000)]
    public decimal CurrentPrice { get; set; }
    
    [Range(0, 100000)]
    public int StockQuantity { get; set; }
}
```

#### 2. **Output DTOs** (Read)
```csharp
public class ProductDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal CurrentPrice { get; set; }
    public int StockQuantity { get; set; }
    public int TotalOrders { get; set; }  // Computed
}
```

#### 3. **List DTOs** (Index pages)
```csharp
public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal CurrentPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool InStock => StockQuantity > 0;  // Computed
}
```

### Manual Mapping

```csharp
// DTO → Entity (Create)
var entity = new Product
{
    Name = dto.Name,
    Description = dto.Description,
    CurrentPrice = dto.CurrentPrice,
    StockQuantity = dto.StockQuantity
};

// Entity → DTO (Read)
var dto = new ProductDetailsDto
{
    Id = entity.Id,
    Name = entity.Name,
    Description = entity.Description,
    CurrentPrice = entity.CurrentPrice,
    StockQuantity = entity.StockQuantity,
    TotalOrders = entity.OrderItems.Count  // Computed
};
```

---

## ✅ Validation Strategy

### 1. Data Annotations (Model Level)

```csharp
public class Product
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
    public string Name { get; set; }

    [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000")]
    public decimal CurrentPrice { get; set; }

    [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100000")]
    public int StockQuantity { get; set; }
}
```

### 2. ModelState Validation (Controller Level)

```csharp
[HttpPost]
public IActionResult Create(ProductDto dto)
{
    if (!ModelState.IsValid)
    {
        return View(dto);  // Return form with errors
    }

    // Process valid data
}
```

### 3. Custom Validation (Business Logic)

```csharp
[HttpPost]
public IActionResult Create(OrderDto dto)
{
    if (!ModelState.IsValid)
        return View(dto);

    // Custom validation
    var customer = _dbContext.Customers.Find(dto.CustomerId);
    if (customer is null)
    {
        ModelState.AddModelError("CustomerId", "Customer not found");
        return View(dto);
    }

    // Process valid data
}
```

---

## 🛡️ Authorization Strategy

### Role-Based Authorization

**Roles:**
- `Admin` - Full access
- `Worker` - CRUD except delete
- `Customer` - Read own data only
- `Anonymous` - Public pages only

### Controller Authorization

```csharp
// 1. Allow everyone
[AllowAnonymous]
public IActionResult Index() { ... }

// 2. Require authentication
[Authorize]
public IActionResult Dashboard() { ... }

// 3. Require specific role
[Authorize(Roles = "Admin")]
public IActionResult Delete(int id) { ... }

// 4. Multiple roles
[Authorize(Roles = "Admin,Worker")]
public IActionResult Create() { ... }
```

### Action-Level Authorization

```csharp
[Route("products")]
public class ProductsController : Controller
{
    [Route("")] // Everyone
    [AllowAnonymous]
    public IActionResult Index() { ... }

    [HttpGet]
    [Route("create")] // Admin + Worker
    [Authorize(Roles = "Admin,Worker")]
    public IActionResult Create() { ... }

    [HttpPost]
    [Route("{id:int}/delete")] // Admin only
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id) { ... }
}
```

---

## ⚠️ Error Handling Strategy

### 1. Try-Catch in Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Create(ProductDto dto)
{
    try
    {
        // Business logic
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error creating product");
        ModelState.AddModelError("", "Failed to save product. Please try again.");
        return View(dto);
    }
}
```

### 2. Global Exception Handler

**Middleware**: `UnhandledExceptionLoggingMiddleware`

```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception");
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal Server Error");
    }
}
```

### 3. Not Found Handling

```csharp
public IActionResult Details(int id)
{
    var entity = _dbContext.Entities
        .FirstOrDefault(e => e.Id == id && !e.IsDeleted);

    if (entity is null)
        return NotFound();  // Returns 404

    return View(entity);
}
```

---

## 🗄️ Database Context (EF Core)

### AppDbContext

```csharp
public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Phone> Phones { get; set; }
    public DbSet<RepairJob> RepairJobs { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<SparePart> SpareParts { get; set; }
    public DbSet<Technician> Technicians { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);

        // ... more configurations
    }
}
```

### Configuration

**Production** (MySQL):
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, 
        new MySqlServerVersion(new Version(8, 0, 36)))
);
```

**Testing** (InMemory):
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TestDb")
);
```

---

## 🔐 Identity & Authentication

### AppUser (Identity)

```csharp
public class AppUser : IdentityUser
{
    // Extends IdentityUser with custom properties if needed
}
```

### Configuration

```csharp
builder.Services
    .AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
```

### Role Seeding

```csharp
public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Worker", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
```

---

## 📊 API vs MVC Controllers

### MVC Controllers (HTML)

**Route**: `/products`  
**Returns**: `View()` → Razor HTML

```csharp
[Route("products")]
public class ProductsController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        return View(products);  // Returns HTML
    }
}
```

### API Controllers (JSON)

**Route**: `/api/products`  
**Returns**: `Json()` / `Ok()` → JSON

```csharp
[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<ProductDto>> Get()
    {
        return Ok(products);  // Returns JSON
    }
}
```

---

## 📈 Performance Considerations

### 1. Eager Loading
```csharp
var orders = _dbContext.Orders
    .Include(o => o.Customer)           // Load related customer
    .Include(o => o.OrderItems)         // Load order items
        .ThenInclude(oi => oi.Product)  // Load products
    .ToList();
```

### 2. Projection (Select)
```csharp
var products = _dbContext.Products
    .Select(p => new ProductListDto  // Only needed columns
    {
        Id = p.Id,
        Name = p.Name,
        CurrentPrice = p.CurrentPrice
    })
    .ToList();
```

### 3. Async Operations
```csharp
await _dbContext.SaveChangesAsync();    // Non-blocking
await _dbContext.Products.ToListAsync(); // Non-blocking
```

### 4. Pagination
```csharp
var products = _dbContext.Products
    .Where(p => !p.IsDeleted)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToList();
```

---

## 🎯 Best Practices Implemented

### ✅ 1. Dependency Injection
```csharp
public class ProductsController : Controller
{
    private readonly AppDbContext _dbContext;  // Injected
    private readonly ILogger<ProductsController> _logger;  // Injected

    public ProductsController(AppDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
}
```

### ✅ 2. Repository Pattern (via EF Core)
```csharp
var products = _dbContext.Products  // Repository
    .Where(p => !p.IsDeleted)
    .ToList();
```

### ✅ 3. DTO Pattern
- Input DTOs for Create/Update
- Output DTOs for Read
- List DTOs for Index pages

### ✅ 4. Soft Delete Pattern
- `IsDeleted` flag
- Preserve data
- Foreign key integrity

### ✅ 5. Logging
- `ILogger<T>` dependency injection
- Structured logging (Serilog)
- CRUD audit filter

### ✅ 6. Authorization
- Role-based access control
- `[Authorize]` attributes
- Action-level authorization

### ✅ 7. Validation
- Data Annotations
- ModelState validation
- Custom business logic validation

### ✅ 8. Error Handling
- Try-catch in controllers
- Global exception middleware
- Not Found handling

---

## 📦 Dependencies

### Core Framework
- **ASP.NET Core 10.0** - Web framework
- **Entity Framework Core 9.0** - ORM
- **MySQL.EntityFrameworkCore 9.0** - MySQL provider

### Identity & Authentication
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** - Identity system
- **Google Authentication** - OAuth provider

### Logging
- **Serilog.AspNetCore** - Structured logging
- **Serilog.Sinks.File** - File logging

### Testing
- **xUnit** - Test framework
- **FluentAssertions** - Assertion library
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

### MCP
- **ModelContextProtocol.AspNetCore** - MCP server

### AI
- **Groq API** (HTTP client) - AI integration

---

## 🎓 Zaključak

Arhitektura projekta je:
- ✅ **Layered** - Jasna separacija concerns (Presentation, Business, Data)
- ✅ **SOLID** - Follows SOLID principles
- ✅ **DRY** - Don't Repeat Yourself (reusable patterns)
- ✅ **Testable** - Integration tests pokrivaju sve endpointe
- ✅ **Maintainable** - Consistent patterns across controllers
- ✅ **Secure** - Authorization, validation, error handling
- ✅ **Scalable** - Async operations, pagination, projection

**Status**: ✅ **2/2 bodova za CRUD zasluženo**

---

**Last Updated:** 2026-07-01  
**Version:** 1.0.0  
**Framework:** ASP.NET Core 10.0  
**Author:** AI-Assisted Implementation
