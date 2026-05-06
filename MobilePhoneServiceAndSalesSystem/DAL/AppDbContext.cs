using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.DAL
{
    public class AppDbContext : DbContext
    {
        protected AppDbContext() {}

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {}

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<RepairJob> RepairJobs { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}