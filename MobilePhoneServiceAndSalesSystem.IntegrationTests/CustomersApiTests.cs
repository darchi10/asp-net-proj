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
    public class CustomersApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public CustomersApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesCustomer()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new CustomerDto
            {
                FirstName = "Ana",
                LastName = "Peric",
                Email = "ana.peric@example.com",
                PhoneNumber = "+38591111222",
                Address = "Example 12, Zagreb"
            };

            var response = await client.PostAsJsonAsync("/api/customers", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<CustomerDetailsDto>();
            created.Should().NotBeNull();
            created!.Email.Should().Be(dto.Email);
            db.Customers.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new CustomerDto
            {
                FirstName = "",
                LastName = "",
                Email = "not-an-email",
                PhoneNumber = "",
                Address = ""
            };

            var response = await client.PostAsJsonAsync("/api/customers", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.Customers.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsCustomers_WithSearch()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            db.Customers.AddRange(
                new Customer
                {
                    FirstName = "Ivan",
                    LastName = "Horvat",
                    Email = "ivan@example.com",
                    PhoneNumber = "123",
                    Address = "Address 1"
                },
                new Customer
                {
                    FirstName = "Mia",
                    LastName = "Matic",
                    Email = "mia@example.com",
                    PhoneNumber = "456",
                    Address = "Address 2"
                }
            );
            db.SaveChanges();

            var response = await client.GetAsync("/api/customers?q=ivan");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<CustomerListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].Email.Should().Be("ivan@example.com");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingCustomer()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/customers/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact]
        public async Task Get_ById_ReturnsCustomer_WhenExists()
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

            var response = await client.GetAsync($"/api/customers/{customer.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<CustomerDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(customer.Id);
        }

        [Fact]
        public async Task Put_UpdatesCustomer()
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

            var dto = new CustomerDto
            {
                FirstName = "Lana",
                LastName = "Basic",
                Email = "lana.new@example.com",
                PhoneNumber = "888",
                Address = "New Address"
            };

            var response = await client.PutAsJsonAsync($"/api/customers/{customer.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<CustomerDetailsDto>();
            updated!.Email.Should().Be(dto.Email);
            db.ChangeTracker.Clear();
            db.Customers.First().Email.Should().Be(dto.Email);
        }


        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingCustomer()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new CustomerDto
            {
                FirstName = "Sara",
                LastName = "Novak",
                Email = "sara.novak@example.com",
                PhoneNumber = "111",
                Address = "Address"
            };

            var response = await client.PutAsJsonAsync("/api/customers/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesCustomer()
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

            var response = await client.DeleteAsync($"/api/customers/{customer.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.Customers.First();
            refreshed.IsDeleted.Should().BeTrue();
        }


        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingCustomer()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/customers/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
