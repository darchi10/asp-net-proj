using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.MCP;

/// <summary>
/// MCP Tools for Repair Job operations - enables AI agents to track, search, and manage repair jobs
/// </summary>
[McpServerToolType]
public sealed class RepairJobTools
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RepairJobTools> _logger;

    public RepairJobTools(AppDbContext dbContext, ILogger<RepairJobTools> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Track the status of a repair job by its ID. Returns detailed information about the repair progress.")]
    public async Task<string> GetRepairJobStatus(
        [Description("The unique ID of the repair job to track")] int jobId,
        CancellationToken ct = default)
    {
        try
        {
            var repairJob = await _dbContext.RepairJobs
                .Where(rj => rj.Id == jobId && !rj.IsDeleted)
                .Include(rj => rj.Phone)
                    .ThenInclude(p => p!.Customer)
                .Include(rj => rj.Technician)
                .Include(rj => rj.UsedParts)
                .FirstOrDefaultAsync(ct);

            if (repairJob == null)
            {
                return $"Repair job with ID {jobId} not found.";
            }

            var result = new
            {
                repairJob.Id,
                Description = repairJob.Description,
                Status = repairJob.Status.ToString(),
                ReceivedDate = repairJob.ReceivedDate,
                CompletedDate = repairJob.CompletedDate,
                LaborCost = repairJob.LaborCost,
                Phone = new
                {
                    repairJob.Phone!.Brand,
                    repairJob.Phone.Model,
                    Owner = $"{repairJob.Phone.Customer!.FirstName} {repairJob.Phone.Customer.LastName}"
                },
                Technician = repairJob.Technician != null ? new
                {
                    Name = $"{repairJob.Technician.FirstName} {repairJob.Technician.LastName}",
                    Specialization = repairJob.Technician.Specialization
                } : null,
                UsedParts = repairJob.UsedParts.Select(sp => new
                {
                    sp.Name,
                    sp.Price,
                    sp.Manufacturer
                }).ToList(),
                TotalPartsCost = repairJob.UsedParts.Sum(sp => sp.Price),
                TotalCost = repairJob.LaborCost + repairJob.UsedParts.Sum(sp => sp.Price),
                IsCompleted = repairJob.Status == RepairStatus.Completed,
                DaysInProgress = repairJob.CompletedDate.HasValue
                    ? (repairJob.CompletedDate.Value - repairJob.ReceivedDate).Days
                    : (DateTime.Now - repairJob.ReceivedDate).Days
            };

            _logger.LogInformation("MCP: GetRepairJobStatus for job ID {JobId}", jobId);

            return System.Text.Json.JsonSerializer.Serialize(result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting repair job status for ID {JobId}", jobId);
            return $"Error getting repair job status: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("List repair jobs with optional filters by status. Returns a summary of matching repair jobs.")]
    public async Task<string> ListRepairJobs(
        [Description("Filter by status: 'Pending', 'InProgress', 'Completed', 'Delivered', 'Cancelled', or 'All'")] string status = "All",
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Sort by: 'receivedDate', 'laborCost', or 'status'")] string sortBy = "receivedDate",
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);

            var query = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted)
                .Include(rj => rj.Phone)
                    .ThenInclude(p => p!.Customer)
                .Include(rj => rj.Technician)
                .AsQueryable();

            // Filter by status
            if (status != "All" && Enum.TryParse<RepairStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(rj => rj.Status == parsedStatus);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "laborcost" => query.OrderByDescending(rj => rj.LaborCost),
                "status" => query.OrderBy(rj => rj.Status),
                _ => query.OrderByDescending(rj => rj.ReceivedDate)
            };

            var repairJobs = await query
                .Take(limit)
                .Select(rj => new
                {
                    rj.Id,
                    Description = rj.Description.Length > 100 
                        ? rj.Description.Substring(0, 100) + "..." 
                        : rj.Description,
                    Status = rj.Status.ToString(),
                    ReceivedDate = rj.ReceivedDate,
                    CompletedDate = rj.CompletedDate,
                    LaborCost = rj.LaborCost,
                    PhoneModel = $"{rj.Phone!.Brand} {rj.Phone.Model}",
                    CustomerName = $"{rj.Phone.Customer!.FirstName} {rj.Phone.Customer.LastName}",
                    TechnicianName = rj.Technician != null ? $"{rj.Technician.FirstName} {rj.Technician.LastName}" : "Not assigned"
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: ListRepairJobs returned {Count} results for status '{Status}'", 
                repairJobs.Count, status);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                count = repairJobs.Count,
                statusFilter = status,
                sortedBy = sortBy,
                jobs = repairJobs
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error listing repair jobs");
            return $"Error listing repair jobs: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Search for repair jobs by phone model, customer name, or description.")]
    public async Task<string> SearchRepairJobs(
        [Description("Search query to match against phone model, customer name, or description")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 10,
        CancellationToken ct = default)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 50);
            var searchTerm = query.Trim().ToLower();

            var repairJobs = await _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted &&
                            (rj.Phone!.Brand.ToLower().Contains(searchTerm) ||
                             rj.Phone.Model.ToLower().Contains(searchTerm) ||
                             rj.Phone.Customer!.FirstName.ToLower().Contains(searchTerm) ||
                             rj.Phone.Customer.LastName.ToLower().Contains(searchTerm) ||
                             rj.Description.ToLower().Contains(searchTerm)))
                .Include(rj => rj.Phone)
                    .ThenInclude(p => p!.Customer)
                .Include(rj => rj.Technician)
                .OrderByDescending(rj => rj.ReceivedDate)
                .Take(limit)
                .Select(rj => new
                {
                    rj.Id,
                    Description = rj.Description.Length > 100
                        ? rj.Description.Substring(0, 100) + "..."
                        : rj.Description,
                    Status = rj.Status.ToString(),
                    ReceivedDate = rj.ReceivedDate,
                    PhoneModel = $"{rj.Phone!.Brand} {rj.Phone.Model}",
                    CustomerName = $"{rj.Phone.Customer!.FirstName} {rj.Phone.Customer.LastName}",
                    TechnicianName = rj.Technician != null ? $"{rj.Technician.FirstName} {rj.Technician.LastName}" : "Not assigned"
                })
                .ToListAsync(ct);

            if (!repairJobs.Any())
            {
                return $"No repair jobs found matching '{query}'.";
            }

            _logger.LogInformation("MCP: SearchRepairJobs returned {Count} results for query '{Query}'", 
                repairJobs.Count, query);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                query = searchTerm,
                count = repairJobs.Count,
                jobs = repairJobs
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error searching repair jobs with query '{Query}'", query);
            return $"Error searching repair jobs: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get statistics and summary information about repair jobs.")]
    public async Task<string> GetRepairJobStatistics(CancellationToken ct = default)
    {
        try
        {
            var totalJobs = await _dbContext.RepairJobs.CountAsync(rj => !rj.IsDeleted, ct);
            var pendingJobs = await _dbContext.RepairJobs.CountAsync(
                rj => !rj.IsDeleted && rj.Status == RepairStatus.Pending, ct);
            var inProgressJobs = await _dbContext.RepairJobs.CountAsync(
                rj => !rj.IsDeleted && rj.Status == RepairStatus.InProgress, ct);
            var completedJobs = await _dbContext.RepairJobs.CountAsync(
                rj => !rj.IsDeleted && rj.Status == RepairStatus.Completed, ct);
            var cancelledJobs = await _dbContext.RepairJobs.CountAsync(
                rj => !rj.IsDeleted && rj.Status == RepairStatus.Cancelled, ct);

            var averageLaborCost = await _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted && rj.Status == RepairStatus.Completed)
                .AverageAsync(rj => (double?)rj.LaborCost, ct) ?? 0;

            var averageDuration = await _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted && rj.Status == RepairStatus.Completed && rj.CompletedDate.HasValue)
                .AverageAsync(rj => (rj.CompletedDate!.Value - rj.ReceivedDate).Days, ct);

            _logger.LogInformation("MCP: GetRepairJobStatistics executed");

            var stats = new
            {
                total = totalJobs,
                byStatus = new
                {
                    pending = pendingJobs,
                    inProgress = inProgressJobs,
                    completed = completedJobs,
                    cancelled = cancelledJobs
                },
                completedJobsStats = new
                {
                    averageLaborCost = Math.Round(averageLaborCost, 2),
                    averageDurationDays = Math.Round(averageDuration, 1)
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(stats,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting repair job statistics");
            return $"Error getting repair job statistics: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get repair jobs assigned to a specific technician.")]
    public async Task<string> GetTechnicianRepairJobs(
        [Description("The unique ID of the technician")] int technicianId,
        [Description("Only include active jobs (Pending or InProgress)")] bool activeOnly = true,
        CancellationToken ct = default)
    {
        try
        {
            var query = _dbContext.RepairJobs
                .Where(rj => !rj.IsDeleted && rj.TechnicianId == technicianId);

            if (activeOnly)
            {
                query = query.Where(rj => rj.Status == RepairStatus.Pending || 
                                         rj.Status == RepairStatus.InProgress);
            }

            var jobs = await query
                .Include(rj => rj.Phone)
                    .ThenInclude(p => p!.Customer)
                .OrderByDescending(rj => rj.ReceivedDate)
                .Select(rj => new
                {
                    rj.Id,
                    Description = rj.Description.Length > 100
                        ? rj.Description.Substring(0, 100) + "..."
                        : rj.Description,
                    Status = rj.Status.ToString(),
                    ReceivedDate = rj.ReceivedDate,
                    CompletedDate = rj.CompletedDate,
                    PhoneModel = $"{rj.Phone!.Brand} {rj.Phone.Model}",
                    CustomerName = $"{rj.Phone.Customer!.FirstName} {rj.Phone.Customer.LastName}"
                })
                .ToListAsync(ct);

            _logger.LogInformation("MCP: GetTechnicianRepairJobs for technician ID {TechnicianId} returned {Count} jobs",
                technicianId, jobs.Count);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                technicianId,
                activeOnly,
                count = jobs.Count,
                jobs
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error getting repair jobs for technician ID {TechnicianId}", technicianId);
            return $"Error getting technician repair jobs: {ex.Message}";
        }
    }
}
