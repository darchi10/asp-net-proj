using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;
using System.Collections.Generic;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

// Defining some baseline data for relations
var technician1 = new Technician { Id = 1, FirstName = "Ivan", LastName = "Horvat", Specialization = "Hardware", HireDate = new DateTime(2020, 5, 1), Salary = 1500m };
var part1 = new SparePart { Id = 1, Name = "iPhone 14 Screen", Manufacturer = "Apple", Price = 150m, StockQuantity = 5 };
var part2 = new SparePart { Id = 2, Name = "Samsung Galaxy S22 Battery", Manufacturer = "Samsung", Price = 50m, StockQuantity = 10 };

var product1 = new Product { Id = 1, Name = "Screen Protector", Description = "Tempered glass", CurrentPrice = 15m, StockQuantity = 20 };
var product2 = new Product { Id = 2, Name = "USB-C Cable", Description = "Fast charging cord", CurrentPrice = 15m, StockQuantity = 50 };

// Building Objects separately for Customer 1
var repairJob1 = new RepairJob { Id = 1, Description = "Smashed screen replacement", Status = RepairStatus.Completed, ReceivedDate = DateTime.Now.AddDays(-10), CompletedDate = DateTime.Now.AddDays(-8), LaborCost = 60m, Technician = technician1, UsedParts = new List<SparePart> { part1 } };
var phone1 = new Phone { Id = 1, Brand = "Apple", Model = "iPhone 14", IMEI = "123456789012345", YearOfManufacture = 2022, OperatingSystem = "iOS", RepairJobs = new List<RepairJob> { repairJob1 } };
var orderItem1 = new OrderItem { Id = 1, Product = product1, Quantity = 1, UnitPrice = 15m };
var order1 = new Order { Id = 1, OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 210m, ShippingAddress = "Ilica 10, Zagreb", OrderItems = new List<OrderItem> { orderItem1 } };

var customer1 = new Customer { Id = 1, FirstName = "Ana", LastName = "Anic", Email = "ana.anic@test.com", PhoneNumber = "0912345678", Address = "Ilica 10, Zagreb", Phones = new List<Phone> { phone1 }, Orders = new List<Order> { order1 } };

// Building Objects separately for Customer 2
var repairJob2 = new RepairJob { Id = 2, Description = "Battery draining fast", Status = RepairStatus.InProgress, ReceivedDate = DateTime.Now.AddDays(-1), LaborCost = 40m, Technician = technician1, UsedParts = new List<SparePart> { part2 } };
var phone2 = new Phone { Id = 2, Brand = "Samsung", Model = "Galaxy S22", IMEI = "987654321098765", YearOfManufacture = 2021, OperatingSystem = "Android", RepairJobs = new List<RepairJob> { repairJob2 } };
var phone3 = new Phone { Id = 3, Brand = "Xiaomi", Model = "Redmi Note 12", IMEI = "543216789054321", YearOfManufacture = 2023, OperatingSystem = "Android", RepairJobs = new List<RepairJob>() };

var customer2 = new Customer { Id = 2, FirstName = "Marko", LastName = "Maric", Email = "marko.m@test.com", PhoneNumber = "0987654321", Address = "Vukovarska 20, Split", Phones = new List<Phone> { phone2, phone3 } };

// Building Objects separately for Customer 3
var repairJob3 = new RepairJob { Id = 3, Description = "Charging port cleaning", Status = RepairStatus.Pending, ReceivedDate = DateTime.Now, LaborCost = 20m, Technician = technician1 };
var phone4 = new Phone { Id = 4, Brand = "Google", Model = "Pixel 7", IMEI = "112233445566778", YearOfManufacture = 2022, OperatingSystem = "Android", RepairJobs = new List<RepairJob> { repairJob3 } };
var orderItem2 = new OrderItem { Id = 2, Product = product2, Quantity = 2, UnitPrice = 15m };
var order2 = new Order { Id = 2, OrderDate = DateTime.Now, TotalAmount = 30m, ShippingAddress = "Zvonimirova 5, Osijek", OrderItems = new List<OrderItem> { orderItem2 } };

var customer3 = new Customer { Id = 3, FirstName = "Petra", LastName = "Peric", Email = "petra.p@test.com", PhoneNumber = "0951122334", Address = "Zvonimirova 5, Osijek", Phones = new List<Phone> { phone4 }, Orders = new List<Order> { order2 } };

var customers = new List<Customer> { customer1, customer2, customer3 };

// --- LINQ Queries ---
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

app.Run();
