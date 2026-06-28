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
    public class SearchApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public SearchApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Search_ReturnsDefaultMenuItems_WhenQueryIsEmpty()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/search");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var results = await response.Content.ReadFromJsonAsync<List<SearchResultDto>>();
            results.Should().NotBeNull();
            results!.Count.Should().BeGreaterThan(0);
            results.All(r => r.Category == "Navigation").Should().BeTrue();
        }

        [Fact]
        public async Task Search_ReturnsFilteredMenuItems_WhenQueryMatchesMenu()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            // "home" matches "Home Page"
            var response = await client.GetAsync("/api/search?q=home");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var results = await response.Content.ReadFromJsonAsync<List<SearchResultDto>>();
            results.Should().NotBeNull();
            results!.Count.Should().Be(1);
            results[0].Title.Should().Be("Home Page");
        }

        [Fact]
        public async Task Search_ReturnsDatabaseEntities_WhenQueryMatchesDatabase()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            // Add test products
            db.Products.AddRange(
                new Product
                {
                    Name = "Samsung Galaxy S24 Ultra",
                    Description = "Flagship smartphone",
                    CurrentPrice = 1200m,
                    StockQuantity = 5
                },
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Apple smartphone",
                    CurrentPrice = 1100m,
                    StockQuantity = 10
                }
            );
            db.SaveChanges();

            // Search for "samsung" (length >= 2)
            var response = await client.GetAsync("/api/search?q=samsung");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var results = await response.Content.ReadFromJsonAsync<List<SearchResultDto>>();
            results.Should().NotBeNull();
            
            // Should find the Samsung product
            var samsungProduct = results!.FirstOrDefault(r => r.Category == "Products");
            samsungProduct.Should().NotBeNull();
            samsungProduct!.Title.Should().Be("Samsung Galaxy S24 Ultra");
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
