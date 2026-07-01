using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.MCP;

/// <summary>
/// MCP Tools for Order operations - enables AI agents to view, search, and track orders
/// </summary>
[McpServerToolType]
public sealed class OrderTools
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderTools> _logger;

    public OrderTools(AppDbContext dbContext, ILogger<OrderTools> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Get detailed information about a specific order by its ID.")]
    public async Task<string> GetOrderDetails(
        [Description("The unique ID of the order")] int orderId,
        CancellationToken ct = default)
    {
        try
        {
            var order = await _dbContext.Orders
                .Where(o => o.Id == orderId && !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(ct);

            if (order == null)
            {
                return $"Order with ID {orderId} not found.";
            }

            var result = new
            {
                order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Customer = new
                {
                    Name = $"{order.Customer!.FirstName} {order.Customer.LastName}",
                    order.Customer.Email,
                    order.Customer.PhoneNumber
                },
                Items = order.OrderItems.Select(oi => new
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Quantity * oi.UnitPrice
                }).ToList(),
                TotalItems = order.OrderItems.Sum(oi => oi.Quantity)
            };

            _logger.LogInformation("MCP: GetOrderDetails for order ID {OrderId}", orderId);

            return System.Text.Json.JsonSerializer.Serialize(result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting order details for ID {OrderId}", orderId);
            return $"Error getting order details: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("List orders with optional filters by customer and date range.")]
    public async Task<string> ListOrders(
        [Description("Filter by customer ID (optional)")] int? customerId = null,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Sort by: 'date' or 'amount'")] string sortBy = "date",
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);

            var query = _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Filter by customer
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "amount" => query.OrderByDescending(o => o.TotalAmount),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            var orders = await query
                .Take(limit)
                .Select(o => new
                {
                    o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    CustomerName = $"{o.Customer!.FirstName} {o.Customer.LastName}",
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    ProductCount = o.OrderItems.Count
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: ListOrders returned {Count} results", orders.Count);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                count = orders.Count,
                customerIdFilter = customerId,
                sortedBy = sortBy,
                orders
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error listing orders");
            return $"Error listing orders: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get all orders for a specific customer by customer ID.")]
    public async Task<string> GetCustomerOrders(
        [Description("The unique ID of the customer")] int customerId,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);

            var customer = await _dbContext.Customers
                .Where(c => c.Id == customerId && !c.IsDeleted)
                .FirstOrDefaultAsync(ct);

            if (customer == null)
            {
                return $"Customer with ID {customerId} not found.";
            }

            var orders = await _dbContext.Orders
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .Select(o => new
                {
                    o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity)
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetCustomerOrders for customer ID {CustomerId} returned {Count} orders",
                customerId, orders.Count);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                customer = new
                {
                    customer.Id,
                    Name = $"{customer.FirstName} {customer.LastName}",
                    customer.Email
                },
                orderCount = orders.Count,
                orders
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting orders for customer ID {CustomerId}", customerId);
            return $"Error getting customer orders: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Search for orders by customer name or product name.")]
    public async Task<string> SearchOrders(
        [Description("Search query to match against customer name or product names in orders")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 10,
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);
            var searchTerm = query.Trim().ToLower();

            var orders = await _dbContext.Orders
                .Where(o => !o.IsDeleted &&
                           (o.Customer!.FirstName.ToLower().Contains(searchTerm) ||
                            o.Customer.LastName.ToLower().Contains(searchTerm) ||
                            o.OrderItems.Any(oi => oi.Product!.Name.ToLower().Contains(searchTerm))))
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .Select(o => new
                {
                    o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    CustomerName = $"{o.Customer!.FirstName} {o.Customer.LastName}",
                    Products = o.OrderItems.Select(oi => oi.Product!.Name).ToList(),
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity)
                })
                .ToListAsync(ct);

            if (!orders.Any())
            {
                return $"No orders found matching '{query}'.";
            }

            _logger.LogInformation("MCP: SearchOrders returned {Count} results for query '{Query}'",
                orders.Count, query);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                query = searchTerm,
                count = orders.Count,
                orders
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error searching orders with query '{Query}'", query);
            return $"Error searching orders: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get statistics and summary information about orders.")]
    public async Task<string> GetOrderStatistics(CancellationToken ct = default)
    {
        try
        {
            var totalOrders = await _dbContext.Orders.CountAsync(o => !o.IsDeleted, ct);
            var totalRevenue = await _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            var averageOrderValue = totalOrders > 0 
                ? await _dbContext.Orders.Where(o => !o.IsDeleted).AverageAsync(o => (double?)o.TotalAmount, ct) ?? 0
                : 0;

            var topProducts = await _dbContext.OrderItems
                .Where(oi => !oi.Order!.IsDeleted)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetOrderStatistics executed");

            var stats = new
            {
                total = totalOrders,
                revenue = new
                {
                    total = Math.Round(totalRevenue, 2),
                    averageOrderValue = Math.Round(averageOrderValue, 2)
                },
                topProducts
            };

            return System.Text.Json.JsonSerializer.Serialize(stats,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting order statistics");
            return $"Error getting order statistics: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get recent orders placed within the specified number of days.")]
    public async Task<string> GetRecentOrders(
        [Description("Number of days to look back (1-90)")] int days = 7,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        CancellationToken ct = default)
    {
        try
        {
            days = Math.Clamp(days, 1, 90);
            limit = Math.Clamp(limit, 1, 50);

            var startDate = DateTime.Now.AddDays(-days);

            var orders = await _dbContext.Orders
                .Where(o => !o.IsDeleted && o.OrderDate >= startDate)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .Select(o => new
                {
                    o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    CustomerName = $"{o.Customer!.FirstName} {o.Customer.LastName}",
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity)
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetRecentOrders returned {Count} orders from last {Days} days",
                orders.Count, days);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                daysBack = days,
                startDate,
                count = orders.Count,
                orders
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting recent orders");
            return $"Error getting recent orders: {ex.Message}";
        }
    }
}
