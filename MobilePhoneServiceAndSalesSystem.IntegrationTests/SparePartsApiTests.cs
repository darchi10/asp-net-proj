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
    public class SparePartsApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public SparePartsApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesSparePart()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new SparePartDto
            {
                Name = "Screen",
                Manufacturer = "Samsung",
                Price = 120.50m,
                StockQuantity = 10
            };

            var response = await client.PostAsJsonAsync("/api/spare-parts", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<SparePartDetailsDto>();
            created.Should().NotBeNull();
            created!.Name.Should().Be(dto.Name);
            db.SpareParts.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new SparePartDto
            {
                Name = "",
                Manufacturer = "",
                Price = 0,
                StockQuantity = -1
            };

            var response = await client.PostAsJsonAsync("/api/spare-parts", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.SpareParts.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsSpareParts_WithSearch()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            db.SpareParts.AddRange(
                new SparePart
                {
                    Name = "Battery",
                    Manufacturer = "Apple",
                    Price = 45.00m,
                    StockQuantity = 5
                },
                new SparePart
                {
                    Name = "Speaker",
                    Manufacturer = "Xiaomi",
                    Price = 15.00m,
                    StockQuantity = 20
                }
            );
            db.SaveChanges();

            var response = await client.GetAsync("/api/spare-parts?q=Battery");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<SparePartListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].Name.Should().Be("Battery");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingSparePart()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/spare-parts/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ById_ReturnsSparePart_WhenExists()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var sparePart = new SparePart
            {
                Name = "Camera",
                Manufacturer = "Samsung",
                Price = 75.00m,
                StockQuantity = 3
            };
            db.SpareParts.Add(sparePart);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/spare-parts/{sparePart.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<SparePartDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(sparePart.Id);
        }

        [Fact]
        public async Task Put_UpdatesSparePart()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var sparePart = new SparePart
            {
                Name = "Microphone",
                Manufacturer = "Nokia",
                Price = 18.00m,
                StockQuantity = 12
            };
            db.SpareParts.Add(sparePart);
            db.SaveChanges();

            var dto = new SparePartDto
            {
                Name = "Microphone",
                Manufacturer = "Nokia",
                Price = 20.00m,
                StockQuantity = 10
            };

            var response = await client.PutAsJsonAsync($"/api/spare-parts/{sparePart.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<SparePartDetailsDto>();
            updated!.Price.Should().Be(dto.Price);
            db.ChangeTracker.Clear();
            db.SpareParts.First().Price.Should().Be(dto.Price);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingSparePart()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new SparePartDto
            {
                Name = "Antenna",
                Manufacturer = "Sony",
                Price = 9.99m,
                StockQuantity = 4
            };

            var response = await client.PutAsJsonAsync("/api/spare-parts/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesSparePart()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var sparePart = new SparePart
            {
                Name = "Charging Port",
                Manufacturer = "LG",
                Price = 7.50m,
                StockQuantity = 8
            };
            db.SpareParts.Add(sparePart);
            db.SaveChanges();

            var response = await client.DeleteAsync($"/api/spare-parts/{sparePart.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.SpareParts.First();
            refreshed.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingSparePart()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/spare-parts/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
