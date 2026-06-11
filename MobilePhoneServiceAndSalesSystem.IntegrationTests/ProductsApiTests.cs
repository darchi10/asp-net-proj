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
    public class ProductsApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public ProductsApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesProduct()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new ProductDto
            {
                Name = "Charging Cable",
                Description = "USB-C",
                CurrentPrice = 10m,
                StockQuantity = 25
            };

            var response = await client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<ProductDetailsDto>();
            created.Should().NotBeNull();
            created!.Name.Should().Be(dto.Name);
            db.Products.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new ProductDto
            {
                Name = "",
                Description = "",
                CurrentPrice = 0m,
                StockQuantity = -1
            };

            var response = await client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.Products.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsProducts_WithSearch()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            db.Products.AddRange(
                new Product
                {
                    Name = "Screen Protector",
                    Description = "Tempered glass",
                    CurrentPrice = 15m,
                    StockQuantity = 10
                },
                new Product
                {
                    Name = "Case",
                    Description = "Protective",
                    CurrentPrice = 20m,
                    StockQuantity = 7
                }
            );
            db.SaveChanges();

            var response = await client.GetAsync("/api/products?q=Protector");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<ProductListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].Name.Should().Be("Screen Protector");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingProduct()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/products/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ById_ReturnsProduct_WhenExists()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var product = new Product
            {
                Name = "Adapter",
                Description = "USB-A",
                CurrentPrice = 12m,
                StockQuantity = 6
            };
            db.Products.Add(product);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/products/{product.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<ProductDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(product.Id);
        }

        [Fact]
        public async Task Put_UpdatesProduct()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var product = new Product
            {
                Name = "Headphones",
                Description = "Wireless",
                CurrentPrice = 50m,
                StockQuantity = 10
            };
            db.Products.Add(product);
            db.SaveChanges();

            var dto = new ProductDto
            {
                Name = "Headphones",
                Description = "Wireless",
                CurrentPrice = 55m,
                StockQuantity = 8
            };

            var response = await client.PutAsJsonAsync($"/api/products/{product.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<ProductDetailsDto>();
            updated!.CurrentPrice.Should().Be(dto.CurrentPrice);
            db.ChangeTracker.Clear();
            db.Products.First().CurrentPrice.Should().Be(dto.CurrentPrice);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingProduct()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new ProductDto
            {
                Name = "Charger",
                Description = "Fast",
                CurrentPrice = 25m,
                StockQuantity = 5
            };

            var response = await client.PutAsJsonAsync("/api/products/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesProduct()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var product = new Product
            {
                Name = "Microphone",
                Description = "Replacement",
                CurrentPrice = 18m,
                StockQuantity = 12
            };
            db.Products.Add(product);
            db.SaveChanges();

            var response = await client.DeleteAsync($"/api/products/{product.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.Products.First();
            refreshed.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingProduct()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/products/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
