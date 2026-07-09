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
using MobilePhoneServiceAndSalesSystem.Models.Enums;
using Xunit;

namespace MobilePhoneServiceAndSalesSystem.IntegrationTests
{
    public class RepairJobsApiTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public RepairJobsApiTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreatesRepairJob()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            var dto = new RepairJobDto
            {
                Description = "Screen repair",
                Status = RepairStatus.Pending,
                ReceivedDate = new DateTime(2024, 1, 10),
                CompletedDate = null,
                LaborCost = 50m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id,
                UsedPartIds = new List<int>()
            };

            var response = await client.PostAsJsonAsync("/api/repair-jobs", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<RepairJobDetailsDto>();
            created.Should().NotBeNull();
            created!.Description.Should().Be(dto.Description);
            db.RepairJobs.Count().Should().Be(1);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_ForInvalidModel()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var dto = new RepairJobDto
            {
                Description = "Bad",
                Status = RepairStatus.Pending,
                ReceivedDate = default,
                CompletedDate = null,
                LaborCost = -1m,
                PhoneId = 0,
                TechnicianId = 0,
                UsedPartIds = new List<int>()
            };

            var response = await client.PostAsJsonAsync("/api/repair-jobs", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.RepairJobs.Count().Should().Be(0);
        }

        [Fact]
        public async Task Get_ReturnsRepairJobs_WithSearch()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            db.RepairJobs.AddRange(
                new RepairJob
                {
                    Description = "Screen repair",
                    Status = RepairStatus.InProgress,
                    ReceivedDate = new DateTime(2024, 1, 10),
                    LaborCost = 50m,
                    PhoneId = phone.Id,
                    TechnicianId = technician.Id
                },
                new RepairJob
                {
                    Description = "Battery replacement",
                    Status = RepairStatus.Pending,
                    ReceivedDate = new DateTime(2024, 1, 11),
                    LaborCost = 30m,
                    PhoneId = phone.Id,
                    TechnicianId = technician.Id
                }
            );
            db.SaveChanges();

            var response = await client.GetAsync("/api/repair-jobs?q=Screen");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<RepairJobListDto>>();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list[0].Description.Should().Be("Screen repair");
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_ForMissingRepairJob()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.GetAsync("/api/repair-jobs/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ById_ReturnsRepairJob_WhenExists()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            var job = new RepairJob
            {
                Description = "Diagnostics",
                Status = RepairStatus.Pending,
                ReceivedDate = new DateTime(2024, 2, 1),
                LaborCost = 25m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };
            db.RepairJobs.Add(job);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/repair-jobs/{job.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var details = await response.Content.ReadFromJsonAsync<RepairJobDetailsDto>();
            details.Should().NotBeNull();
            details!.Id.Should().Be(job.Id);
        }

        [Fact]
        public async Task Put_UpdatesRepairJob()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            var job = new RepairJob
            {
                Description = "Diagnostics",
                Status = RepairStatus.Pending,
                ReceivedDate = new DateTime(2024, 2, 1),
                LaborCost = 25m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };
            db.RepairJobs.Add(job);
            db.SaveChanges();

            var dto = new RepairJobDto
            {
                Description = "Diagnostics and repair",
                Status = RepairStatus.InProgress,
                ReceivedDate = job.ReceivedDate,
                CompletedDate = null,
                LaborCost = 40m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id,
                UsedPartIds = new List<int>()
            };

            var response = await client.PutAsJsonAsync($"/api/repair-jobs/{job.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<RepairJobDetailsDto>();
            updated!.Description.Should().Be(dto.Description);
            db.ChangeTracker.Clear();
            db.RepairJobs.First().Description.Should().Be(dto.Description);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_ForMissingRepairJob()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            var dto = new RepairJobDto
            {
                Description = "Diagnostics",
                Status = RepairStatus.Pending,
                ReceivedDate = new DateTime(2024, 2, 1),
                CompletedDate = null,
                LaborCost = 25m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id,
                UsedPartIds = new List<int>()
            };

            var response = await client.PutAsJsonAsync("/api/repair-jobs/999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_SoftDeletesRepairJob()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            var job = new RepairJob
            {
                Description = "Screen repair",
                Status = RepairStatus.Pending,
                ReceivedDate = new DateTime(2024, 1, 10),
                LaborCost = 50m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };
            db.RepairJobs.Add(job);
            db.SaveChanges();

            var response = await client.DeleteAsync($"/api/repair-jobs/{job.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            db.ChangeTracker.Clear();
            var refreshed = db.RepairJobs.First();
            refreshed.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForMissingRepairJob()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);

            var response = await client.DeleteAsync("/api/repair-jobs/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenCompletedDateIsBeforeReceivedDate()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);
            var (phone, technician) = SeedRepairDependencies(db);
            var now = DateTime.Now;

            var dto = new RepairJobDto
            {
                Description = "Screen replacement",
                Status = RepairStatus.Completed,
                ReceivedDate = now.AddDays(-1),
                CompletedDate = now.AddDays(-2),
                LaborCost = 50m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };

            var response = await client.PostAsJsonAsync("/api/repair-jobs", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.RepairJobs.Should().BeEmpty();
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenActiveRepairHasCompletedDate()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);
            var (phone, technician) = SeedRepairDependencies(db);
            var now = DateTime.Now;

            var dto = new RepairJobDto
            {
                Description = "Battery replacement",
                Status = RepairStatus.InProgress,
                ReceivedDate = now.AddDays(-2),
                CompletedDate = now.AddDays(-1),
                LaborCost = 30m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };

            var response = await client.PostAsJsonAsync("/api/repair-jobs", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.RepairJobs.Should().BeEmpty();
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenCompletedRepairHasNoCompletedDate()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);
            var (phone, technician) = SeedRepairDependencies(db);

            var dto = new RepairJobDto
            {
                Description = "Charging port repair",
                Status = RepairStatus.Completed,
                ReceivedDate = DateTime.Now.AddDays(-2),
                CompletedDate = null,
                LaborCost = 45m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };

            var response = await client.PostAsJsonAsync("/api/repair-jobs", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.RepairJobs.Should().BeEmpty();
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenReceivedDateIsInFuture()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);
            var (phone, technician) = SeedRepairDependencies(db);

            var dto = new RepairJobDto
            {
                Description = "Camera module repair",
                Status = RepairStatus.Pending,
                ReceivedDate = DateTime.Now.AddDays(1),
                CompletedDate = null,
                LaborCost = 40m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };

            var response = await client.PostAsJsonAsync("/api/repair-jobs", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.RepairJobs.Should().BeEmpty();
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_WhenStatusTransitionSkipsWorkflow()
        {
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(db);
            var (phone, technician) = SeedRepairDependencies(db);
            var job = new RepairJob
            {
                Description = "Display diagnostics",
                Status = RepairStatus.Pending,
                ReceivedDate = DateTime.Now.AddDays(-2),
                LaborCost = 25m,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };
            db.RepairJobs.Add(job);
            db.SaveChanges();

            var dto = new RepairJobDto
            {
                Description = job.Description,
                Status = RepairStatus.Delivered,
                ReceivedDate = job.ReceivedDate,
                CompletedDate = DateTime.Now.AddDays(-1),
                LaborCost = job.LaborCost,
                PhoneId = phone.Id,
                TechnicianId = technician.Id
            };

            var response = await client.PutAsJsonAsync($"/api/repair-jobs/{job.Id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            db.ChangeTracker.Clear();
            db.RepairJobs.Single().Status.Should().Be(RepairStatus.Pending);
        }

        private static Customer CreateCustomer()
        {
            return new Customer
            {
                FirstName = "Ana",
                LastName = "Peric",
                Email = "ana.peric@example.com",
                PhoneNumber = "+38591111222",
                Address = "Example 12, Zagreb"
            };
        }

        private static (Phone Phone, Technician Technician) SeedRepairDependencies(AppDbContext db)
        {
            var customer = CreateCustomer();
            db.Customers.Add(customer);

            var phone = CreatePhone(customer.Id);
            db.Phones.Add(phone);

            var technician = CreateTechnician();
            db.Technicians.Add(technician);
            db.SaveChanges();

            return (phone, technician);
        }

        private static Phone CreatePhone(int customerId)
        {
            return new Phone
            {
                Brand = "Apple",
                Model = "iPhone 15",
                IMEI = "123456789012345",
                YearOfManufacture = 2024,
                OperatingSystem = "iOS",
                CustomerId = customerId
            };
        }

        private static Technician CreateTechnician()
        {
            return new Technician
            {
                FirstName = "Ivan",
                LastName = "Horvat",
                Specialization = "Diagnostics",
                HireDate = new DateTime(2021, 5, 10),
                Salary = 1200m
            };
        }

        private static void ResetDatabase(AppDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
