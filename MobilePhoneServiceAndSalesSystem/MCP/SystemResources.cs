using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.MCP;

/// <summary>
/// MCP Resources for read-only data - provides AI agents with system statistics, logs, and reports
/// </summary>
[McpServerToolType]
public sealed class SystemResources
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SystemResources> _logger;

    public SystemResources(AppDbContext dbContext, ILogger<SystemResources> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Get comprehensive dashboard statistics including products, orders, repairs, and customers.")]
    public async Task<string> GetDashboardStatistics(CancellationToken ct = default)
    {
        try
        {
            // Products statistics
            var totalProducts = await _dbContext.Products.CountAsync(p => !p.IsDeleted, ct);
            var inStockProducts = await _dbContext.Products.CountAsync(p => !p.IsDeleted && p.StockQuantity > 0, ct);
            var lowStockProducts = await _dbContext.Products.CountAsync(p => !p.IsDeleted && p.StockQuantity > 0 && p.StockQuantity < 5, ct);
            var outOfStockProducts = await _dbContext.Products.CountAsync(p => !p.IsDeleted && p.StockQuantity == 0, ct);

            // Orders statistics
            var totalOrders = await _dbContext.Orders.CountAsync(o => !o.IsDeleted, ct);
            var totalRevenue = await _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            // Repair jobs statistics
            var totalRepairJobs = await _dbContext.RepairJobs.CountAsync(rj => !rj.IsDeleted, ct);
            var pendingRepairs = await _dbContext.RepairJobs.CountAsync(rj => !rj.IsDeleted && rj.Status == RepairStatus.Pending, ct);
            var inProgressRepairs = await _dbContext.RepairJobs.CountAsync(rj => !rj.IsDeleted && rj.Status == RepairStatus.InProgress, ct);
            var completedRepairs = await _dbContext.RepairJobs.CountAsync(rj => !rj.IsDeleted && rj.Status == RepairStatus.Completed, ct);

            // Customer statistics
            var totalCustomers = await _dbContext.Customers.CountAsync(c => !c.IsDeleted, ct);
            var activeCustomers = await _dbContext.Customers
                .Where(c => !c.IsDeleted && (c.Orders.Any() || c.Phones.Any(p => p.RepairJobs.Any())))
                .CountAsync(ct);

            // Technician statistics
            var totalTechnicians = await _dbContext.Technicians.CountAsync(t => !t.IsDeleted, ct);
            var activeTechnicians = await _dbContext.Technicians
                .Where(t => !t.IsDeleted && t.RepairJobs.Any(rj => rj.Status == RepairStatus.InProgress))
                .CountAsync(ct);

            _logger.LogInformation("MCP: GetDashboardStatistics executed");

            var stats = new
            {
                timestamp = DateTime.Now,
                products = new
                {
                    total = totalProducts,
                    inStock = inStockProducts,
                    lowStock = lowStockProducts,
                    outOfStock = outOfStockProducts
                },
                orders = new
                {
                    total = totalOrders,
                    totalRevenue = Math.Round(totalRevenue, 2)
                },
                repairJobs = new
                {
                    total = totalRepairJobs,
                    pending = pendingRepairs,
                    inProgress = inProgressRepairs,
                    completed = completedRepairs
                },
                customers = new
                {
                    total = totalCustomers,
                    active = activeCustomers
                },
                technicians = new
                {
                    total = totalTechnicians,
                    currentlyWorking = activeTechnicians
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(stats,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting dashboard statistics");
            return $"Error getting dashboard statistics: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get inventory status report showing product stock levels and alerts.")]
    public async Task<string> GetInventoryReport(CancellationToken ct = default)
    {
        try
        {
            var inventory = await _dbContext.Products
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    Stock = p.StockQuantity,
                    Price = p.CurrentPrice,
                    StockValue = p.StockQuantity * p.CurrentPrice,
                    Status = p.StockQuantity == 0 ? "Out of Stock" :
                             p.StockQuantity < 5 ? "Low Stock" : "In Stock",
                    Alert = p.StockQuantity < 5
                })
                .OrderBy(p => p.Stock)
                .ToListAsync(ct);

            var totalStockValue = inventory.Sum(i => i.StockValue);
            var alertProducts = inventory.Count(i => i.Alert);

            _logger.LogInformation("MCP: GetInventoryReport executed");

            var report = new
            {
                timestamp = DateTime.Now,
                summary = new
                {
                    totalProducts = inventory.Count,
                    totalStockValue = Math.Round(totalStockValue, 2),
                    productsNeedingAttention = alertProducts
                },
                inventory
            };

            return System.Text.Json.JsonSerializer.Serialize(report,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting inventory report");
            return $"Error getting inventory report: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get technician workload report showing job assignments and performance.")]
    public async Task<string> GetTechnicianWorkloadReport(CancellationToken ct = default)
    {
        try
        {
            var technicians = await _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Select(t => new
                {
                    t.Id,
                    Name = $"{t.FirstName} {t.LastName}",
                    t.Specialization,
                    TotalAssignedJobs = t.RepairJobs.Count(rj => !rj.IsDeleted),
                    ActiveJobs = t.RepairJobs.Count(rj => !rj.IsDeleted && 
                        (rj.Status == RepairStatus.Pending || rj.Status == RepairStatus.InProgress)),
                    CompletedJobs = t.RepairJobs.Count(rj => !rj.IsDeleted && rj.Status == RepairStatus.Completed),
                    WorkloadStatus = t.RepairJobs.Count(rj => !rj.IsDeleted && 
                        (rj.Status == RepairStatus.Pending || rj.Status == RepairStatus.InProgress)) > 5 
                        ? "Heavy" : "Normal"
                })
                .OrderByDescending(t => t.ActiveJobs)
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetTechnicianWorkloadReport executed");

            var report = new
            {
                timestamp = DateTime.Now,
                summary = new
                {
                    totalTechnicians = technicians.Count,
                    totalActiveJobs = technicians.Sum(t => t.ActiveJobs),
                    techniciansWithHeavyLoad = technicians.Count(t => t.WorkloadStatus == "Heavy")
                },
                technicians
            };

            return System.Text.Json.JsonSerializer.Serialize(report,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting technician workload report");
            return $"Error getting technician workload report: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get sales performance report for a specified time period.")]
    public async Task<string> GetSalesReport(
        [Description("Number of days to look back (1-365)")] int days = 30,
        CancellationToken ct = default)
    {
        try
        {
            days = Math.Clamp(days, 1, 365);
            var startDate = DateTime.Now.AddDays(-days);

            var orders = await _dbContext.Orders
                .Where(o => !o.IsDeleted && o.OrderDate >= startDate)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync(ct);

            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count;
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var productSales = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();

            var dailySales = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Orders = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(d => d.Date)
                .ToList();

            _logger.LogInformation("MCP: GetSalesReport for last {Days} days executed", days);

            var report = new
            {
                timestamp = DateTime.Now,
                period = new
                {
                    startDate,
                    endDate = DateTime.Now,
                    days
                },
                summary = new
                {
                    totalRevenue = Math.Round(totalRevenue, 2),
                    totalOrders,
                    averageOrderValue = Math.Round(averageOrderValue, 2)
                },
                topProducts = productSales,
                dailySales = dailySales.Take(7)
            };

            return System.Text.Json.JsonSerializer.Serialize(report,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting sales report");
            return $"Error getting sales report: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get customer activity report showing most active customers and their spending.")]
    public async Task<string> GetCustomerActivityReport(
        [Description("Maximum number of customers to return (1-50)")] int limit = 20,
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);

            var customerActivity = await _dbContext.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    c.Id,
                    Name = $"{c.FirstName} {c.LastName}",
                    c.Email,
                    c.PhoneNumber,
                    TotalOrders = c.Orders.Count(o => !o.IsDeleted),
                    TotalSpent = c.Orders.Where(o => !o.IsDeleted).Sum(o => o.TotalAmount),
                    TotalRepairJobs = c.Phones.SelectMany(p => p.RepairJobs).Count(rj => !rj.IsDeleted),
                    LastOrderDate = c.Orders.Where(o => !o.IsDeleted).Max(o => (DateTime?)o.OrderDate),
                    ActivityScore = (c.Orders.Count(o => !o.IsDeleted) * 10) + 
                                   c.Phones.SelectMany(p => p.RepairJobs).Count(rj => !rj.IsDeleted) * 5
                })
                .OrderByDescending(c => c.ActivityScore)
                .Take(limit)
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetCustomerActivityReport executed");

            var report = new
            {
                timestamp = DateTime.Now,
                summary = new
                {
                    totalCustomersAnalyzed = customerActivity.Count,
                    topCustomerSpending = customerActivity.FirstOrDefault()?.TotalSpent ?? 0
                },
                customers = customerActivity
            };

            return System.Text.Json.JsonSerializer.Serialize(report,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting customer activity report");
            return $"Error getting customer activity report: {ex.Message}";
        }
    }
}
