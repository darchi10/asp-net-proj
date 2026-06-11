using System;
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
    public class OrdersApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public OrdersApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesOrder()
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

            var product = new Product
            {
                Name = "Charging Cable",
                Description = "USB-C",
                CurrentPrice = 10m,
                StockQuantity = 25
            };
            db.Products.Add(product);
            db.SaveChanges();

            var dto = new OrderDto
            {
                CustomerId = customer.Id,
                ShippingAddress = "Example 12, Zagreb",
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 2
                    }
                }
            };

            var response = await client.PostAsJsonAsync("/api/orders", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<OrderDetailsDto>();
            created.Should().NotBeNull();
            created!.CustomerId.Should().Be(dto.CustomerId);
            db.Orders.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new OrderDto
            {
                CustomerId = 0,
                ShippingAddress = "",
                OrderItems = new List<OrderItemDto>()
            };

            var response = await client.PostAsJsonAsync("/api/orders", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.Orders.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsOrders_WithSearch()
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

            var product = new Product
            {
                Name = "Screen Protector",
                Description = "Tempered glass",
                CurrentPrice = 15m,
                StockQuantity = 10
            };
            db.Products.Add(product);
            db.SaveChanges();

            var order = new Order
            {
                CustomerId = customer.Id,
                ShippingAddress = "Zagreb Center",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 15m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = 15m
                    }
                }
            };
            db.Orders.Add(order);
            db.SaveChanges();

            var response = await client.GetAsync("/api/orders?q=Zagreb");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<OrderListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].ShippingAddress.Should().Be("Zagreb Center");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingOrder()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/orders/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ById_ReturnsOrder_WhenExists()
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

            var product = new Product
            {
                Name = "Case",
                Description = "Protective",
                CurrentPrice = 20m,
                StockQuantity = 7
            };
            db.Products.Add(product);
            db.SaveChanges();

            var order = new Order
            {
                CustomerId = customer.Id,
                ShippingAddress = "Address",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 40m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 2,
                        UnitPrice = 20m
                    }
                }
            };
            db.Orders.Add(order);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/orders/{order.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<OrderDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(order.Id);
        }

        [Fact]
        public async Task Put_UpdatesOrder()
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

            var product = new Product
            {
                Name = "Headphones",
                Description = "Wireless",
                CurrentPrice = 50m,
                StockQuantity = 10
            };
            db.Products.Add(product);
            db.SaveChanges();

            var order = new Order
            {
                CustomerId = customer.Id,
                ShippingAddress = "Old Address",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 50m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = 50m
                    }
                }
            };
            db.Orders.Add(order);
            db.SaveChanges();

            var dto = new OrderDto
            {
                CustomerId = customer.Id,
                ShippingAddress = "New Address",
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 2
                    }
                }
            };

            var response = await client.PutAsJsonAsync($"/api/orders/{order.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<OrderDetailsDto>();
            updated!.ShippingAddress.Should().Be(dto.ShippingAddress);
            db.ChangeTracker.Clear();
            db.Orders.First().ShippingAddress.Should().Be(dto.ShippingAddress);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingOrder()
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

            var product = new Product
            {
                Name = "Charger",
                Description = "Fast",
                CurrentPrice = 25m,
                StockQuantity = 5
            };
            db.Products.Add(product);
            db.SaveChanges();

            var dto = new OrderDto
            {
                CustomerId = customer.Id,
                ShippingAddress = "Address",
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 1
                    }
                }
            };

            var response = await client.PutAsJsonAsync("/api/orders/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesOrder()
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

            var product = new Product
            {
                Name = "Adapter",
                Description = "USB-A",
                CurrentPrice = 12m,
                StockQuantity = 6
            };
            db.Products.Add(product);
            db.SaveChanges();

            var order = new Order
            {
                CustomerId = customer.Id,
                ShippingAddress = "Address",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 12m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = 12m
                    }
                }
            };
            db.Orders.Add(order);
            db.SaveChanges();

            var response = await client.DeleteAsync($"/api/orders/{order.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.Orders.First();
            refreshed.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingOrder()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/orders/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
