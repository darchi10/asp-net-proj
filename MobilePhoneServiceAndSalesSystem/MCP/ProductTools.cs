using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using MobilePhoneServiceAndSalesSystem.DAL;

namespace MobilePhoneServiceAndSalesSystem.MCP;

/// <summary>
/// MCP Tools for Product operations - enables AI agents to search, view, and manage products
/// </summary>
[McpServerToolType]
public sealed class ProductTools
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProductTools> _logger;

    public ProductTools(AppDbContext dbContext, ILogger<ProductTools> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Search for products by name or description. Returns matching products with their details.")]
    public async Task<string> SearchProducts(
        [Description("Search query to match against product name and description")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 10,
        [Description("Only return products that are in stock")] bool inStockOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);
            var searchTerm = query.Trim();

            var products = await _dbContext.Products
                .Where(p => !p.IsDeleted &&
                            (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)) &&
                            (!inStockOnly || p.StockQuantity > 0))
                .OrderByDescending(p => p.StockQuantity)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    Price = p.CurrentPrice,
                    Stock = p.StockQuantity,
                    InStock = p.StockQuantity > 0
                })
                .ToListAsync(ct);

            if (!products.Any())
            {
                return $"No products found matching '{query}'.";
            }

            _logger.LogInformation("MCP: SearchProducts returned {Count} results for query '{Query}'", products.Count, query);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                query = searchTerm,
                count = products.Count,
                products
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error searching products with query '{Query}'", query);
            return $"Error searching products: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get detailed information about a specific product by its ID.")]
    public async Task<string> GetProductDetails(
        [Description("The unique ID of the product")] int productId,
        CancellationToken ct = default)
    {
        try
        {
            var product = await _dbContext.Products
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Include(p => p.OrderItems)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    Price = p.CurrentPrice,
                    Stock = p.StockQuantity,
                    InStock = p.StockQuantity > 0,
                    TotalOrders = p.OrderItems.Count
                })
                .FirstOrDefaultAsync(ct);

            if (product == null)
            {
                return $"Product with ID {productId} not found.";
            }

            _logger.LogInformation("MCP: GetProductDetails for product ID {ProductId}", productId);

            return System.Text.Json.JsonSerializer.Serialize(product, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting product details for ID {ProductId}", productId);
            return $"Error getting product details: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Check stock availability for a specific product.")]
    public async Task<string> CheckProductStock(
        [Description("The unique ID of the product")] int productId,
        CancellationToken ct = default)
    {
        try
        {
            var product = await _dbContext.Products
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    Stock = p.StockQuantity,
                    InStock = p.StockQuantity > 0,
                    StockStatus = p.StockQuantity == 0 ? "Out of Stock" :
                                  p.StockQuantity < 5 ? "Low Stock" : "In Stock"
                })
                .FirstOrDefaultAsync(ct);

            if (product == null)
            {
                return $"Product with ID {productId} not found.";
            }

            _logger.LogInformation("MCP: CheckProductStock for product ID {ProductId}", productId);

            return System.Text.Json.JsonSerializer.Serialize(product,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error checking stock for product ID {ProductId}", productId);
            return $"Error checking product stock: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get all available products, optionally filtered by stock availability.")]
    public async Task<string> ListProducts(
        [Description("Maximum number of products to return (1-100)")] int limit = 20,
        [Description("Only return products that are in stock")] bool inStockOnly = false,
        [Description("Sort by: 'name', 'price', or 'stock'")] string sortBy = "name",
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 100);

            var query = _dbContext.Products
                .Where(p => !p.IsDeleted && (!inStockOnly || p.StockQuantity > 0));

            query = sortBy.ToLower() switch
            {
                "price" => query.OrderBy(p => p.CurrentPrice),
                "stock" => query.OrderByDescending(p => p.StockQuantity),
                _ => query.OrderBy(p => p.Name)
            };

            var products = await query
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    Price = p.CurrentPrice,
                    Stock = p.StockQuantity,
                    InStock = p.StockQuantity > 0
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: ListProducts returned {Count} results", products.Count);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                count = products.Count,
                sortedBy = sortBy,
                inStockOnly,
                products
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error listing products");
            return $"Error listing products: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get products that are low in stock (less than 5 units) or out of stock.")]
    public async Task<string> GetLowStockProducts(
        [Description("Stock threshold - products with stock below this number (1-20)")] int threshold = 5,
        CancellationToken ct = default)
    {
        try
        {
            threshold = Math.Clamp(threshold, 1, 20);

            var products = await _dbContext.Products
                .Where(p => !p.IsDeleted && p.StockQuantity < threshold)
                .OrderBy(p => p.StockQuantity)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    Stock = p.StockQuantity,
                    Price = p.CurrentPrice,
                    Status = p.StockQuantity == 0 ? "Out of Stock" : "Low Stock"
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetLowStockProducts found {Count} products below threshold {Threshold}", 
                products.Count, threshold);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                threshold,
                count = products.Count,
                products
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting low stock products");
            return $"Error getting low stock products: {ex.Message}";
        }
    }
}
