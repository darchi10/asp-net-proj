using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.DTOs;
using Xunit;

namespace MobilePhoneServiceAndSalesSystem.IntegrationTests
{
    public class PhonesApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public PhonesApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesPhone()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = new Customer
            {
                FirstName = "Ana",
                LastName = "Peric",
                Email = "ana.peric@example.com",
                PhoneNumber = "+38591111222",
                Address = "Example 12, Zagreb"
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var dto = new PhoneDto
            {
                Brand = "Apple",
                Model = "iPhone 15",
                IMEI = "123456789012345",
                YearOfManufacture = 2024,
                OperatingSystem = "iOS",
                CustomerId = customer.Id
            };

            var response = await client.PostAsJsonAsync("/api/phones", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<PhoneDetailsDto>();
            created.Should().NotBeNull();
            created!.IMEI.Should().Be(dto.IMEI);
            db.Phones.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new PhoneDto
            {
                Brand = "",
                Model = "",
                IMEI = "",
                YearOfManufacture = 1800,
                OperatingSystem = "",
                CustomerId = 0
            };

            var response = await client.PostAsJsonAsync("/api/phones", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.Phones.Count().Should().Be(0);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForMissingCustomer()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new PhoneDto
            {
                Brand = "Samsung",
                Model = "S24",
                IMEI = "999999999999999",
                YearOfManufacture = 2024,
                OperatingSystem = "Android",
                CustomerId = 999
            };

            var response = await client.PostAsJsonAsync("/api/phones", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.Phones.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsPhones_WithSearch()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = new Customer
            {
                FirstName = "Ivan",
                LastName = "Horvat",
                Email = "ivan@example.com",
                PhoneNumber = "123",
                Address = "Address 1"
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            db.Phones.AddRange(
                new Phone
                {
                    Brand = "Apple",
                    Model = "iPhone 12",
                    IMEI = "111111111111111",
                    YearOfManufacture = 2020,
                    OperatingSystem = "iOS",
                    CustomerId = customer.Id
                },
                new Phone
                {
                    Brand = "Samsung",
                    Model = "Galaxy S23",
                    IMEI = "222222222222222",
                    YearOfManufacture = 2023,
                    OperatingSystem = "Android",
                    CustomerId = customer.Id
                }
            );
            db.SaveChanges();

            var response = await client.GetAsync("/api/phones?q=iPhone");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<PhoneListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].Model.Should().Be("iPhone 12");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingPhone()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/phones/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ById_ReturnsPhone_WhenExists()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = new Customer
            {
                FirstName = "Ivana",
                LastName = "Kovac",
                Email = "ivana.kovac@example.com",
                PhoneNumber = "999",
                Address = "Address"
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var phone = new Phone
            {
                Brand = "Google",
                Model = "Pixel 8",
                IMEI = "555555555555555",
                YearOfManufacture = 2024,
                OperatingSystem = "Android",
                CustomerId = customer.Id
            };
            db.Phones.Add(phone);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/phones/{phone.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<PhoneDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(phone.Id);
        }

        [Fact]
        public async Task Put_UpdatesPhone()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = new Customer
            {
                FirstName = "Lana",
                LastName = "Basic",
                Email = "lana@example.com",
                PhoneNumber = "777",
                Address = "Old Address"
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var phone = new Phone
            {
                Brand = "Xiaomi",
                Model = "Mi 10",
                IMEI = "333333333333333",
                YearOfManufacture = 2021,
                OperatingSystem = "Android",
                CustomerId = customer.Id
            };
            db.Phones.Add(phone);
            db.SaveChanges();

            var dto = new PhoneDto
            {
                Brand = "Xiaomi",
                Model = "Mi 11",
                IMEI = "333333333333333",
                YearOfManufacture = 2022,
                OperatingSystem = "Android",
                CustomerId = customer.Id
            };

            var response = await client.PutAsJsonAsync($"/api/phones/{phone.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<PhoneDetailsDto>();
            updated!.Model.Should().Be(dto.Model);
            db.ChangeTracker.Clear();
            db.Phones.First().Model.Should().Be(dto.Model);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingPhone()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = new Customer
            {
                FirstName = "Sara",
                LastName = "Novak",
                Email = "sara.novak@example.com",
                PhoneNumber = "111",
                Address = "Address"
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var dto = new PhoneDto
            {
                Brand = "Sony",
                Model = "Xperia 1",
                IMEI = "666666666666666",
                YearOfManufacture = 2023,
                OperatingSystem = "Android",
                CustomerId = customer.Id
            };

            var response = await client.PutAsJsonAsync("/api/phones/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesPhone()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = new Customer
            {
                FirstName = "Marko",
                LastName = "Novak",
                Email = "marko@example.com",
                PhoneNumber = "555",
                Address = "Address"
            };
            db.Customers.Add(customer);
            db.SaveChanges();

            var phone = new Phone
            {
                Brand = "Nokia",
                Model = "3310",
                IMEI = "444444444444444",
                YearOfManufacture = 2000,
                OperatingSystem = "Series 30",
                CustomerId = customer.Id
            };
            db.Phones.Add(phone);
            db.SaveChanges();

            var response = await client.DeleteAsync($"/api/phones/{phone.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.Phones.First();
            refreshed.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingPhone()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/phones/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
