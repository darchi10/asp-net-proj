using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Configure default culture to use Euro for currency
var defaultCulture = new CultureInfo("en-IE");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

// Add services to the container.
builder.Services.AddControllersWithViews();

var customerRepo = new CustomerMockRepository();
var phoneRepo = new PhoneMockRepository();
var orderRepo = new OrderMockRepository();
var productRepo = new ProductMockRepository();
var repairJobRepo = new RepairJobMockRepository();
var sparePartRepo = new SparePartMockRepository();
var technicianRepo = new TechnicianMockRepository();

builder.Services.AddSingleton<ICustomerMockRepository>(customerRepo);
builder.Services.AddSingleton<IPhoneMockRepository>(phoneRepo);
builder.Services.AddSingleton<IOrderMockRepository>(orderRepo);
builder.Services.AddSingleton<IProductMockRepository>(productRepo);
builder.Services.AddSingleton<IRepairJobMockRepository>(repairJobRepo);
builder.Services.AddSingleton<ISparePartMockRepository>(sparePartRepo);
builder.Services.AddSingleton<ITechnicianMockRepository>(technicianRepo);

Technician technician1 = new Technician { Id = 1, FirstName = "Ivan", LastName = "Horvat", Specialization = "Hardware", HireDate = new DateTime(2020, 5, 1), Salary = 1500m };
technicianRepo.Add(technician1);

SparePart part1 = new SparePart { Id = 1, Name = "iPhone 14 Screen", Manufacturer = "Apple", Price = 150m, StockQuantity = 5 };
SparePart part2 = new SparePart { Id = 2, Name = "Samsung Galaxy S22 Battery", Manufacturer = "Samsung", Price = 50m, StockQuantity = 10 };
sparePartRepo.Add(part1);
sparePartRepo.Add(part2);

Product product1 = new Product { Id = 1, Name = "Screen Protector", Description = "Tempered glass", CurrentPrice = 15m, StockQuantity = 20 };
Product product2 = new Product { Id = 2, Name = "USB-C Cable", Description = "Fast charging cord", CurrentPrice = 15m, StockQuantity = 50 };
productRepo.Add(product1);
productRepo.Add(product2);

Customer customer1 = new Customer { Id = 1, FirstName = "Ana", LastName = "Anic", Email = "ana.anic@test.com", PhoneNumber = "0912345678", Address = "Ilica 10, Zagreb" };
customerRepo.Add(customer1);

RepairJob repairJob1 = new RepairJob { Id = 1, Description = "Smashed screen replacement", Status = RepairStatus.Completed, ReceivedDate = DateTime.Now.AddDays(-10), CompletedDate = DateTime.Now.AddDays(-8), LaborCost = 60m, Technician = technician1, TechnicianId = technician1.Id, UsedParts = new List<SparePart> { part1 } };
repairJobRepo.Add(repairJob1);

Phone phone1 = new Phone { Id = 1, Brand = "Apple", Model = "iPhone 14", IMEI = "123456789012345", YearOfManufacture = 2022, OperatingSystem = "iOS", Customer = customer1, CustomerId = customer1.Id, RepairJobs = new List<RepairJob> { repairJob1 } };
phoneRepo.Add(phone1);

repairJob1.Phone = phone1;
repairJob1.PhoneId = phone1.Id;

OrderItem orderItem1 = new OrderItem { Id = 1, Product = product1, ProductId = product1.Id, Quantity = 1, UnitPrice = 15m };
Order order1 = new Order { Id = 1, OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 210m, ShippingAddress = "Ilica 10, Zagreb", Customer = customer1, CustomerId = customer1.Id, OrderItems = new List<OrderItem> { orderItem1 } };
orderItem1.Order = order1;
orderItem1.OrderId = order1.Id;
orderRepo.Add(order1);

product1.OrderItems.Add(orderItem1);
part1.RepairJobs.Add(repairJob1);
technician1.RepairJobs.Add(repairJob1);
customer1.Phones.Add(phone1);
customer1.Orders.Add(order1);

Customer customer2 = new Customer { Id = 2, FirstName = "Marko", LastName = "Maric", Email = "marko.m@test.com", PhoneNumber = "0987654321", Address = "Vukovarska 20, Split" };
customerRepo.Add(customer2);

RepairJob repairJob2 = new RepairJob { Id = 2, Description = "Battery draining fast", Status = RepairStatus.InProgress, ReceivedDate = DateTime.Now.AddDays(-1), LaborCost = 40m, Technician = technician1, TechnicianId = technician1.Id, UsedParts = new List<SparePart> { part2 } };
repairJobRepo.Add(repairJob2);

Phone phone2 = new Phone { Id = 2, Brand = "Samsung", Model = "Galaxy S22", IMEI = "987654321098765", YearOfManufacture = 2021, OperatingSystem = "Android", Customer = customer2, CustomerId = customer2.Id, RepairJobs = new List<RepairJob> { repairJob2 } };
Phone phone3 = new Phone { Id = 3, Brand = "Xiaomi", Model = "Redmi Note 12", IMEI = "543216789054321", YearOfManufacture = 2023, OperatingSystem = "Android", Customer = customer2, CustomerId = customer2.Id, RepairJobs = new List<RepairJob>() };
phoneRepo.Add(phone2);
phoneRepo.Add(phone3);

repairJob2.Phone = phone2;
repairJob2.PhoneId = phone2.Id;

part2.RepairJobs.Add(repairJob2);
technician1.RepairJobs.Add(repairJob2);
customer2.Phones.Add(phone2);
customer2.Phones.Add(phone3);

Customer customer3 = new Customer { Id = 3, FirstName = "Petra", LastName = "Peric", Email = "petra.p@test.com", PhoneNumber = "0951122334", Address = "Zvonimirova 5, Osijek" };
customerRepo.Add(customer3);

RepairJob repairJob3 = new RepairJob { Id = 3, Description = "Charging port cleaning", Status = RepairStatus.Pending, ReceivedDate = DateTime.Now, LaborCost = 20m, Technician = technician1, TechnicianId = technician1.Id };
repairJobRepo.Add(repairJob3);

Phone phone4 = new Phone { Id = 4, Brand = "Google", Model = "Pixel 7", IMEI = "112233445566778", YearOfManufacture = 2022, OperatingSystem = "Android", Customer = customer3, CustomerId = customer3.Id, RepairJobs = new List<RepairJob> { repairJob3 } };
phoneRepo.Add(phone4);

repairJob3.Phone = phone4;
repairJob3.PhoneId = phone4.Id;

OrderItem orderItem2 = new OrderItem { Id = 2, Product = product2, ProductId = product2.Id, Quantity = 2, UnitPrice = 15m };
Order order2 = new Order { Id = 2, OrderDate = DateTime.Now, TotalAmount = 30m, ShippingAddress = "Zvonimirova 5, Osijek", Customer = customer3, CustomerId = customer3.Id, OrderItems = new List<OrderItem> { orderItem2 } };
orderItem2.Order = order2;
orderItem2.OrderId = order2.Id;
orderRepo.Add(order2);

product2.OrderItems.Add(orderItem2);
technician1.RepairJobs.Add(repairJob3);
customer3.Phones.Add(phone4);
customer3.Orders.Add(order2);

List<Customer> customers = customerRepo.Items.ToList();

Console.WriteLine("\n--- LINQ REZULTATI ---");

var activeRepairJobs = customers
    .SelectMany(c => c.Phones.SelectMany(p => p.RepairJobs.Select(rj => new 
    { 
        CustomerName = $"{c.FirstName} {c.LastName}", 
        PhoneInfo = $"{p.Brand} {p.Model}", 
        Job = rj 
    })))
    .Where(x => x.Job.Status == RepairStatus.Pending || x.Job.Status == RepairStatus.InProgress)
    .OrderBy(x => x.Job.ReceivedDate)
    .ToList();

Console.WriteLine("\n[1] AKTIVNI SERVISI ZA ODRADITI (Dashboard):");
foreach (var item in activeRepairJobs)
{
    Console.WriteLine($"- Zaprimljeno: {item.Job.ReceivedDate.ToShortDateString()} | Uređaj: {item.PhoneInfo} | Kvar: {item.Job.Description} | Status: {item.Job.Status} | Kupac: {item.CustomerName}");
}


var topCustomersRevenue = customers
    .Select(c => new 
    {
        FullName = $"{c.FirstName} {c.LastName}",
        TotalOrdersAmount = c.Orders.Sum(o => o.TotalAmount),
        TotalRepairsAmount = c.Phones.SelectMany(p => p.RepairJobs).Where(rj => rj.Status == RepairStatus.Completed).Sum(rj => rj.LaborCost),
        GrandTotalSpent = c.Orders.Sum(o => o.TotalAmount) + c.Phones.SelectMany(p => p.RepairJobs).Where(rj => rj.Status == RepairStatus.Completed).Sum(rj => rj.LaborCost)
    })
    .OrderByDescending(x => x.GrandTotalSpent)
    .ToList();

Console.WriteLine("\n[2] TOP KUPCI PREMA POTROŠNJI (Marketing / Loyalty):");
foreach (var c in topCustomersRevenue)
{
    Console.WriteLine($"- Kupac: {c.FullName} | Ukupno ostavio: {c.GrandTotalSpent} EUR (Od toga narudžbe: {c.TotalOrdersAmount} EUR, Servisi: {c.TotalRepairsAmount} EUR)");
}

var app = builder.Build();

app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
