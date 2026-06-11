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
    public class TechniciansApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public TechniciansApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesTechnician()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new TechnicianDto
            {
                FirstName = "Ana",
                LastName = "Peric",
                Specialization = "Screen repair",
                HireDate = new DateTime(2021, 5, 10),
                Salary = 1200m
            };

            var response = await client.PostAsJsonAsync("/api/technicians", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<TechnicianDetailsDto>();
            created.Should().NotBeNull();
            created!.LastName.Should().Be(dto.LastName);
            db.Technicians.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new TechnicianDto
            {
                FirstName = "",
                LastName = "",
                Specialization = "",
                HireDate = default,
                Salary = -1m
            };

            var response = await client.PostAsJsonAsync("/api/technicians", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.Technicians.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsTechnicians_WithSearch()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            db.Technicians.AddRange(
                new Technician
                {
                    FirstName = "Ivan",
                    LastName = "Horvat",
                    Specialization = "Battery replacement",
                    HireDate = new DateTime(2020, 1, 15),
                    Salary = 1100m
                },
                new Technician
                {
                    FirstName = "Mia",
                    LastName = "Matic",
                    Specialization = "Soldering",
                    HireDate = new DateTime(2019, 3, 20),
                    Salary = 1300m
                }
            );
            db.SaveChanges();

            var response = await client.GetAsync("/api/technicians?q=Horvat");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<TechnicianListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].LastName.Should().Be("Horvat");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingTechnician()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/technicians/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ById_ReturnsTechnician_WhenExists()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var technician = new Technician
            {
                FirstName = "Ivana",
                LastName = "Kovac",
                Specialization = "Diagnostics",
                HireDate = new DateTime(2022, 2, 2),
                Salary = 1400m
            };
            db.Technicians.Add(technician);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/technicians/{technician.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<TechnicianDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(technician.Id);
        }

        [Fact]
        public async Task Put_UpdatesTechnician()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var technician = new Technician
            {
                FirstName = "Lana",
                LastName = "Basic",
                Specialization = "Hardware",
                HireDate = new DateTime(2020, 6, 1),
                Salary = 1000m
            };
            db.Technicians.Add(technician);
            db.SaveChanges();

            var dto = new TechnicianDto
            {
                FirstName = "Lana",
                LastName = "Basic",
                Specialization = "Micro-soldering",
                HireDate = technician.HireDate,
                Salary = 1500m
            };

            var response = await client.PutAsJsonAsync($"/api/technicians/{technician.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<TechnicianDetailsDto>();
            updated!.Specialization.Should().Be(dto.Specialization);
            db.ChangeTracker.Clear();
            db.Technicians.First().Specialization.Should().Be(dto.Specialization);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingTechnician()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new TechnicianDto
            {
                FirstName = "Sara",
                LastName = "Novak",
                Specialization = "Diagnostics",
                HireDate = new DateTime(2021, 8, 15),
                Salary = 900m
            };

            var response = await client.PutAsJsonAsync("/api/technicians/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesTechnician()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var technician = new Technician
            {
                FirstName = "Marko",
                LastName = "Novak",
                Specialization = "Screen repair",
                HireDate = new DateTime(2018, 9, 1),
                Salary = 950m
            };
            db.Technicians.Add(technician);
            db.SaveChanges();

            var response = await client.DeleteAsync($"/api/technicians/{technician.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.Technicians.First();
            refreshed.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingTechnician()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/technicians/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
