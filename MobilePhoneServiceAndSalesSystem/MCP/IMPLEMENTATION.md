# MCP (Model Context Protocol) Implementation Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Implementation Details](#implementation-details)
4. [Available Tools](#available-tools)
5. [Setup & Configuration](#setup--configuration)
6. [Usage Examples](#usage-examples)
7. [Troubleshooting](#troubleshooting)
8. [Technical Details](#technical-details)

---

## Overview

The Mobile Phone Service and Sales System exposes its business logic through a **Model Context Protocol (MCP)** server, enabling AI agents to interact with the system programmatically. This allows AI assistants like Kiro CLI, Claude Desktop, Cursor, and VS Code Copilot to query products, track repair jobs, manage orders, and access system statistics.

### What is MCP?

**Model Context Protocol (MCP)** is an open standard created by Anthropic that enables AI applications to connect to external tools, data sources, and services through a standardized JSON-RPC 2.0 interface. Think of it as "USB-C for AI" - one universal protocol that any AI agent can use to access any service.

### Why MCP?

- ✅ **Universal Interface**: Write once, use with any MCP-compatible AI agent
- ✅ **Standardized Protocol**: No custom integration needed for each AI client
- ✅ **Automatic Discovery**: AI agents discover tools and capabilities automatically
- ✅ **Type-Safe**: JSON Schema ensures proper parameter validation
- ✅ **Real-time**: Direct access to live data without APIs

---

## Architecture

### High-Level Architecture

```
┌─────────────────────┐
│   AI Agent          │
│ (Kiro, Claude, etc) │
└──────────┬──────────┘
           │ HTTP/SSE
           │ JSON-RPC 2.0
           ▼
┌─────────────────────┐
│  MCP Server         │
│  (ASP.NET Core)     │
│                     │
│  ┌───────────────┐  │
│  │ MCP Tools     │  │
│  │ - Products    │  │
│  │ - RepairJobs  │  │
│  │ - Orders      │  │
│  │ - System      │  │
│  └───────────────┘  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Database           │
│  (MySQL)            │
└─────────────────────┘
```

### Component Breakdown

#### 1. **MCP Endpoint** (`/mcp`)
- Handles JSON-RPC 2.0 requests from AI agents
- Uses Streamable HTTP transport with Server-Sent Events (SSE)
- Configured via `ModelContextProtocol.AspNetCore` NuGet package

#### 2. **OAuth Metadata Endpoint** (`/.well-known/oauth-authorization-server`)
- Provides OAuth 2.0 server metadata for MCP discovery
- Returns minimal metadata indicating no authentication required (for development)
- Allows MCP clients to successfully connect without OAuth complexity

#### 3. **MCP Tools Classes**
Four specialized tool classes expose business logic:
- `ProductTools.cs` - Product management (5 tools)
- `RepairJobTools.cs` - Repair job tracking (5 tools)
- `OrderTools.cs` - Order management (6 tools)
- `SystemResources.cs` - System statistics and reports (5 tools)

---

## Implementation Details

### 1. NuGet Package

**Package:** `ModelContextProtocol.AspNetCore` (v1.4.0)

Provides:
- HTTP transport implementation (Streamable HTTP + SSE)
- JSON-RPC 2.0 message handling
- Tool discovery and registration
- Automatic schema generation from attributes

### 2. Program.cs Configuration

```csharp
// Add MCP services
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Map MCP endpoint
app.MapMcp("/mcp");
```

**Explanation:**
- `AddMcpServer()` - Registers MCP services in DI container
- `WithHttpTransport()` - Configures Streamable HTTP transport
- `WithToolsFromAssembly()` - Auto-discovers tools marked with `[McpServerToolType]`
- `MapMcp("/mcp")` - Maps endpoint to `/mcp` route

### 3. Tool Implementation Pattern

Each tool follows this pattern:

```csharp
[McpServerToolType]
public sealed class ExampleTools
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ExampleTools> _logger;

    public ExampleTools(AppDbContext dbContext, ILogger<ExampleTools> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Human-readable description of what this tool does")]
    public async Task<string> ToolName(
        [Description("Parameter description")] string param1,
        [Description("Parameter description")] int param2 = 10,
        CancellationToken ct = default)
    {
        try
        {
            // Business logic here
            var result = await _dbContext.SomeData
                .Where(x => x.Matches(param1))
                .Take(param2)
                .ToListAsync(ct);

            _logger.LogInformation("MCP: ToolName executed with {Param1}", param1);

            return System.Text.Json.JsonSerializer.Serialize(result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP: Error in ToolName");
            return $"Error: {ex.Message}";
        }
    }
}
```

**Key Points:**
- `[McpServerToolType]` - Marks class as containing MCP tools
- `[McpServerTool]` - Marks method as an MCP tool
- `[Description]` - Provides human-readable descriptions for AI agents
- Returns JSON strings for structured data
- Includes error handling and logging
- Uses `CancellationToken` for cancellation support

### 4. OAuth Metadata Controller

To enable MCP clients to connect without OAuth complexity, we implemented a minimal OAuth metadata endpoint:

```csharp
[ApiController]
public class OAuthMetadataController : ControllerBase
{
    [HttpGet("/.well-known/oauth-authorization-server")]
    public IActionResult GetAuthorizationServerMetadata()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var metadata = new
        {
            issuer = baseUrl,
            token_endpoint = $"{baseUrl}/oauth/token",
            authorization_endpoint = $"{baseUrl}/oauth/authorize",
            registration_endpoint = $"{baseUrl}/oauth/register",
            grant_types_supported = new string[] { },
            response_types_supported = new string[] { },
            token_endpoint_auth_methods_supported = new[] { "none" },
            code_challenge_methods_supported = new string[] { },
            scopes_supported = new string[] { }
        };

        return Ok(metadata);
    }
}
```

**Why This is Needed:**
- MCP clients (Kiro CLI, Claude Desktop) automatically attempt OAuth discovery
- Without this endpoint, clients fail with "OAuth discovery failed"
- This minimal implementation signals "no authentication required"
- Allows development/testing without OAuth complexity

---

## Available Tools

### Product Tools (5 tools)

Located in `MCP/ProductTools.cs`

#### 1. `SearchProducts`
Search for products by name or description.
- **Parameters:**
  - `query` (string): Search query
  - `limit` (int, 1-50): Max results (default: 10)
  - `inStockOnly` (bool): Only return in-stock products (default: false)

#### 2. `GetProductDetails`
Get detailed information about a specific product.
- **Parameters:**
  - `productId` (int): Unique product ID

#### 3. `CheckProductStock`
Check stock availability for a product.
- **Parameters:**
  - `productId` (int): Unique product ID

#### 4. `ListProducts`
Get all available products.
- **Parameters:**
  - `limit` (int, 1-100): Max results (default: 20)
  - `inStockOnly` (bool): Filter in-stock only (default: false)
  - `sortBy` (string): Sort by 'name', 'price', or 'stock' (default: 'name')

#### 5. `GetLowStockProducts`
Get products that are low in stock or out of stock.
- **Parameters:**
  - `threshold` (int, 1-20): Stock threshold (default: 5)

---

### Repair Job Tools (5 tools)

Located in `MCP/RepairJobTools.cs`

#### 1. `GetRepairJobStatus`
Track the status of a repair job.
- **Parameters:**
  - `jobId` (int): Unique repair job ID
- **Returns:** Complete repair job details including phone, customer, technician, used parts, costs

#### 2. `ListRepairJobs`
List repair jobs with optional filters.
- **Parameters:**
  - `status` (string): 'Pending', 'InProgress', 'Completed', 'Delivered', 'Cancelled', or 'All' (default: 'All')
  - `limit` (int, 1-50): Max results (default: 20)
  - `sortBy` (string): 'receivedDate', 'laborCost', or 'status' (default: 'receivedDate')

#### 3. `SearchRepairJobs`
Search repair jobs by phone model, customer name, or description.
- **Parameters:**
  - `query` (string): Search query
  - `limit` (int, 1-50): Max results (default: 10)

#### 4. `GetRepairJobStatistics`
Get statistics about repair jobs.
- **Returns:** Total jobs, by status breakdown, average labor cost, average duration

#### 5. `GetTechnicianRepairJobs`
Get repair jobs assigned to a specific technician.
- **Parameters:**
  - `technicianId` (int): Unique technician ID
  - `activeOnly` (bool): Only active jobs (default: true)

---

### Order Tools (6 tools)

Located in `MCP/OrderTools.cs`

#### 1. `GetOrderDetails`
Get detailed information about a specific order.
- **Parameters:**
  - `orderId` (int): Unique order ID

#### 2. `ListOrders`
List orders with optional filters.
- **Parameters:**
  - `customerId` (int, optional): Filter by customer
  - `limit` (int, 1-50): Max results (default: 20)
  - `sortBy` (string): 'date' or 'amount' (default: 'date')

#### 3. `GetCustomerOrders`
Get all orders for a specific customer.
- **Parameters:**
  - `customerId` (int): Unique customer ID
  - `limit` (int, 1-50): Max results (default: 20)

#### 4. `SearchOrders`
Search orders by customer name or product name.
- **Parameters:**
  - `query` (string): Search query
  - `limit` (int, 1-50): Max results (default: 10)

#### 5. `GetOrderStatistics`
Get statistics about orders.
- **Returns:** Total orders, revenue, average order value, top products

#### 6. `GetRecentOrders`
Get recent orders within specified days.
- **Parameters:**
  - `days` (int, 1-90): Days to look back (default: 7)
  - `limit` (int, 1-50): Max results (default: 20)

---

### System Resources (5 tools)

Located in `MCP/SystemResources.cs`

#### 1. `GetDashboardStatistics`
Get comprehensive dashboard statistics.
- **Returns:** Statistics for products, orders, repairs, customers, technicians

#### 2. `GetInventoryReport`
Get inventory status report.
- **Returns:** Stock levels, alerts, total stock value

#### 3. `GetTechnicianWorkloadReport`
Get technician workload report.
- **Returns:** Job assignments, active/completed jobs, workload status

#### 4. `GetSalesReport`
Get sales performance report.
- **Parameters:**
  - `days` (int, 1-365): Days to look back (default: 30)

#### 5. `GetCustomerActivityReport`
Get customer activity report.
- **Parameters:**
  - `limit` (int, 1-50): Max customers (default: 20)

---

## Setup & Configuration

### Prerequisites

- .NET 10 SDK
- Running MySQL database
- MCP-compatible AI client (Kiro CLI, Claude Desktop, Cursor, etc.)

### Step 1: Run the Application

```bash
cd MobilePhoneServiceAndSalesSystem
dotnet run
```

Application starts on `http://localhost:5135` (or port specified in `launchSettings.json`)

### Step 2: Verify Endpoints

**Check MCP endpoint:**
```bash
curl http://localhost:5135/mcp
```
Expected: 406 Not Acceptable (normal - requires proper MCP headers)

**Check OAuth metadata:**
```bash
curl http://localhost:5135/.well-known/oauth-authorization-server
```
Expected: JSON with OAuth metadata

### Step 3: Configure MCP Client

#### For Kiro CLI:

```bash
# Add MCP server
kiro-cli mcp add --name mobile-service --url "http://localhost:5135/mcp"

# Verify status
kiro-cli mcp status --name mobile-service

# List configured servers
kiro-cli mcp list

# Start chat
kiro-cli chat
```

#### For Claude Desktop:

Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "mobile-service": {
      "url": "http://localhost:5135/mcp"
    }
  }
}
```

#### For Cursor IDE:

Add to `.cursor/mcp.json`:

```json
{
  "servers": {
    "mobile-service": {
      "url": "http://localhost:5135/mcp"
    }
  }
}
```

---

## Usage Examples

### Example 1: Search for Products

**User Query:**
```
"Show me all iPhone products that are in stock"
```

**AI Agent Calls:**
```json
{
  "tool": "SearchProducts",
  "args": {
    "query": "iPhone",
    "inStockOnly": true,
    "limit": 50
  }
}
```

**Response:**
```json
{
  "query": "iphone",
  "count": 2,
  "products": [
    {
      "Id": 1,
      "Name": "iPhone 15 Pro",
      "Description": "Latest iPhone with titanium design",
      "Price": 1299.99,
      "Stock": 5,
      "InStock": true
    },
    {
      "Id": 2,
      "Name": "iPhone 15",
      "Description": "Standard iPhone 15",
      "Price": 999.99,
      "Stock": 12,
      "InStock": true
    }
  ]
}
```

### Example 2: Track Repair Job

**User Query:**
```
"What's the status of repair job #42?"
```

**AI Agent Calls:**
```json
{
  "tool": "GetRepairJobStatus",
  "args": {
    "jobId": 42
  }
}
```

**Response:**
```json
{
  "Id": 42,
  "Description": "Screen replacement for cracked display",
  "Status": "InProgress",
  "ReceivedDate": "2026-06-20T10:30:00",
  "CompletedDate": null,
  "LaborCost": 50.00,
  "Phone": {
    "Brand": "Samsung",
    "Model": "Galaxy S24",
    "Owner": "John Doe"
  },
  "Technician": {
    "Name": "Jane Smith",
    "Specialization": "Screen Repairs"
  },
  "UsedParts": [
    {
      "Name": "Galaxy S24 Screen",
      "Price": 120.00,
      "Manufacturer": "Samsung"
    }
  ],
  "TotalPartsCost": 120.00,
  "TotalCost": 170.00,
  "IsCompleted": false,
  "DaysInProgress": 9
}
```

### Example 3: Get System Overview

**User Query:**
```
"Give me an overview of the system status"
```

**AI Agent Calls:**
```json
{
  "tool": "GetDashboardStatistics"
}
```

**Response:**
```json
{
  "timestamp": "2026-07-01T21:00:00",
  "products": {
    "total": 45,
    "inStock": 38,
    "lowStock": 5,
    "outOfStock": 2
  },
  "orders": {
    "total": 234,
    "totalRevenue": 45678.90
  },
  "repairJobs": {
    "total": 156,
    "pending": 12,
    "inProgress": 8,
    "completed": 130
  },
  "customers": {
    "total": 89,
    "active": 67
  },
  "technicians": {
    "total": 5,
    "currentlyWorking": 3
  }
}
```

---

## Troubleshooting

### Problem: "OAuth discovery failed"

**Symptom:**
```
OAuth discovery failed: the server does not advertise OAuth endpoints
```

**Solution:**
1. Verify OAuth metadata endpoint is accessible:
   ```bash
   curl http://localhost:5135/.well-known/oauth-authorization-server
   ```
2. Ensure `OAuthMetadataController.cs` is present and built
3. Restart application after adding the controller
4. Re-add MCP server in Kiro:
   ```bash
   kiro-cli mcp remove --name mobile-service
   kiro-cli mcp add --name mobile-service --url "http://localhost:5135/mcp"
   ```

### Problem: Wrong Port

**Symptom:**
```
Unable to connect to localhost:5000
```

**Solution:**
Check `launchSettings.json` for actual port (usually 5135 or 7069 for HTTPS). Use the HTTP port.

### Problem: MCP Tools Not Available in Chat

**Symptom:**
AI agent says tools don't exist or can't call them.

**Solution:**
1. Start a **new chat session** after configuring MCP server
2. MCP tools are loaded at session start, not dynamically
3. Verify server is running:
   ```bash
   kiro-cli mcp status --name mobile-service
   ```

### Problem: Application Not Running

**Symptom:**
```
Connection refused
```

**Solution:**
```bash
cd MobilePhoneServiceAndSalesSystem
dotnet run
```

Keep application running in a separate terminal.

### Problem: 406 Not Acceptable on /mcp

**Symptom:**
Direct `curl http://localhost:5135/mcp` returns 406.

**Solution:**
This is **normal**! MCP endpoint requires specific headers and JSON-RPC format. MCP clients handle this automatically.

---

## Technical Details

### Protocol Specifications

- **Protocol:** JSON-RPC 2.0
- **Transport:** Streamable HTTP with Server-Sent Events (SSE)
- **MCP Specification Version:** 2025-11-25
- **Package:** ModelContextProtocol.AspNetCore 1.4.0
- **Framework:** .NET 10.0

### How It Works

1. **Discovery:** AI agent discovers MCP server via configuration
2. **OAuth Metadata:** Client queries `/.well-known/oauth-authorization-server`
3. **Connection:** Client opens HTTP connection to `/mcp`
4. **Tool Discovery:** Client sends `tools/list` JSON-RPC request
5. **Tool Invocation:** Client sends `tools/call` with tool name and parameters
6. **Response:** Server executes tool and returns JSON result
7. **SSE Streaming:** Long-running operations can stream updates via SSE

### Security Considerations

⚠️ **Current Implementation:**
- **No authentication** - suitable for development only
- **No authorization** - all tools accessible to any client
- **Local access only** - not exposed to internet

🔒 **For Production:**
1. Implement proper OAuth 2.1 authentication
2. Add role-based authorization per tool
3. Use HTTPS only
4. Implement rate limiting
5. Add audit logging
6. Validate all inputs
7. Use API keys or tokens

### Performance

- **Tool calls:** Sub-100ms for simple queries
- **Database queries:** Uses EF Core with includes for efficiency
- **JSON serialization:** System.Text.Json for performance
- **Logging:** Serilog for structured logs
- **Cancellation:** Full support via CancellationToken

### Logging

All MCP tool calls are logged with:
- Tool name
- Parameters (sanitized)
- Execution time
- Result status
- Errors (if any)

Check logs in `logs/` directory.

---

## Future Enhancements

### Potential Improvements

1. **Write Operations**
   - Add create/update/delete tools
   - Require authentication and authorization
   
2. **Real Authentication**
   - Implement OAuth 2.1 properly
   - Support multiple auth providers
   
3. **Advanced Features**
   - Batch operations
   - Webhooks for real-time updates
   - Pagination for large datasets
   - Caching for frequently accessed data
   
4. **Additional Tools**
   - Export reports (PDF, Excel)
   - Email notifications
   - SMS integration
   - Payment processing

5. **Monitoring**
   - Prometheus metrics
   - Health checks
   - Performance monitoring

---

## References

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Kiro CLI Documentation](https://kiro.dev/docs/)
- [ModelContextProtocol.AspNetCore](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore/)
- [OAuth 2.1 Specification](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-12)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)

---

**Last Updated:** 2026-07-01  
**Version:** 1.0.0  
**MCP Spec Version:** 2025-11-25  
**Author:** AI-Assisted Implementation
