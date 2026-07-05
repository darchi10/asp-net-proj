# MCP (Model Context Protocol) Server Documentation

## Overview

The Mobile Phone Service and Sales System exposes its business logic through a **Model Context Protocol (MCP)** server, enabling AI agents (Claude Desktop, Cursor, VS Code Copilot, etc.) to interact with the system programmatically.

**MCP Endpoint:** `/mcp`

## What is MCP?

Model Context Protocol (MCP) is an open standard that enables AI applications to connect to external tools, data sources, and services through a standardized interface. It uses JSON-RPC 2.0 over HTTP/SSE (Server-Sent Events).

## Available Tools

### Product Tools (5 tools)

#### 1. SearchProducts
Search for products by name or description.
- **Parameters:**
  - `query` (string): Search query
  - `limit` (int, 1-50): Max results (default: 10)
  - `inStockOnly` (bool): Only return in-stock products (default: false)
- **Returns:** JSON array of matching products with ID, name, description, price, stock status

#### 2. GetProductDetails
Get detailed information about a specific product.
- **Parameters:**
  - `productId` (int): Unique product ID
- **Returns:** Product details including total orders

#### 3. CheckProductStock
Check stock availability for a product.
- **Parameters:**
  - `productId` (int): Unique product ID
- **Returns:** Stock quantity and status (Out of Stock / Low Stock / In Stock)

#### 4. ListProducts
Get all available products.
- **Parameters:**
  - `limit` (int, 1-100): Max results (default: 20)
  - `inStockOnly` (bool): Filter in-stock only (default: false)
  - `sortBy` (string): Sort by 'name', 'price', or 'stock' (default: 'name')
- **Returns:** List of products

#### 5. GetLowStockProducts
Get products that are low in stock or out of stock.
- **Parameters:**
  - `threshold` (int, 1-20): Stock threshold (default: 5)
- **Returns:** Products below threshold

---

### Repair Job Tools (5 tools)

#### 1. GetRepairJobStatus
Track the status of a repair job.
- **Parameters:**
  - `jobId` (int): Unique repair job ID
- **Returns:** Complete repair job details including phone, customer, technician, used parts, costs

#### 2. ListRepairJobs
List repair jobs with optional filters.
- **Parameters:**
  - `status` (string): 'Pending', 'InProgress', 'Completed', 'Delivered', 'Cancelled', or 'All' (default: 'All')
  - `limit` (int, 1-50): Max results (default: 20)
  - `sortBy` (string): 'receivedDate', 'laborCost', or 'status' (default: 'receivedDate')
- **Returns:** List of repair jobs

#### 3. SearchRepairJobs
Search repair jobs by phone model, customer name, or description.
- **Parameters:**
  - `query` (string): Search query
  - `limit` (int, 1-50): Max results (default: 10)
- **Returns:** Matching repair jobs

#### 4. GetRepairJobStatistics
Get statistics about repair jobs.
- **Parameters:** None
- **Returns:** Total jobs, by status breakdown, average labor cost, average duration

#### 5. GetTechnicianRepairJobs
Get repair jobs assigned to a specific technician.
- **Parameters:**
  - `technicianId` (int): Unique technician ID
  - `activeOnly` (bool): Only active jobs (default: true)
- **Returns:** Technician's repair jobs

---

### Order Tools (7 tools)

#### 1. GetOrderDetails
Get detailed information about a specific order.
- **Parameters:**
  - `orderId` (int): Unique order ID
- **Returns:** Order details with customer info, items, quantities, prices

#### 2. ListOrders
List orders with optional filters.
- **Parameters:**
  - `customerId` (int, optional): Filter by customer
  - `limit` (int, 1-50): Max results (default: 20)
  - `sortBy` (string): 'date' or 'amount' (default: 'date')
- **Returns:** List of orders

#### 3. GetCustomerOrders
Get all orders for a specific customer.
- **Parameters:**
  - `customerId` (int): Unique customer ID
  - `limit` (int, 1-50): Max results (default: 20)
- **Returns:** Customer info and their orders

#### 4. SearchOrders
Search orders by customer name or product name.
- **Parameters:**
  - `query` (string): Search query
  - `limit` (int, 1-50): Max results (default: 10)
- **Returns:** Matching orders

#### 5. GetOrderStatistics
Get statistics about orders.
- **Parameters:** None
- **Returns:** Total orders, revenue, average order value, top products

#### 6. GetRecentOrders
Get recent orders within specified days.
- **Parameters:**
  - `days` (int, 1-90): Days to look back (default: 7)
  - `limit` (int, 1-50): Max results (default: 20)
- **Returns:** Recent orders

---

### System Resources (5 tools)

#### 1. GetDashboardStatistics
Get comprehensive dashboard statistics.
- **Parameters:** None
- **Returns:** Statistics for products, orders, repairs, customers, technicians

#### 2. GetInventoryReport
Get inventory status report.
- **Parameters:** None
- **Returns:** Stock levels, alerts, total stock value

#### 3. GetTechnicianWorkloadReport
Get technician workload report.
- **Parameters:** None
- **Returns:** Job assignments, active/completed jobs, workload status

#### 4. GetSalesReport
Get sales performance report.
- **Parameters:**
  - `days` (int, 1-365): Days to look back (default: 30)
- **Returns:** Revenue, order statistics, top products, daily sales

#### 5. GetCustomerActivityReport
Get customer activity report.
- **Parameters:**
  - `limit` (int, 1-50): Max customers (default: 20)
- **Returns:** Active customers, spending, activity scores

---

## Connecting from AI Clients

### Claude Desktop

Add to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "mobile-service": {
      "url": "http://localhost:5000/mcp",
      "type": "http"
    }
  }
}
```

### Cursor IDE

Add to `.cursor/mcp.json`:

```json
{
  "servers": {
    "mobile-service": {
      "url": "http://localhost:5000/mcp",
      "type": "http"
    }
  }
}
```

### VS Code Copilot

Configure in settings or workspace `.vscode/mcp.json`:

```json
{
  "mcp.servers": [
    {
      "name": "Mobile Service",
      "url": "http://localhost:5000/mcp"
    }
  ]
}
```

---

## Example Usage

### Example 1: Search for iPhone products in stock

**Query to AI Agent:**
```
"Show me all iPhone products that are in stock"
```

**AI Agent calls:**
```json
{
  "tool": "SearchProducts",
  "args": {
    "query": "iPhone",
    "inStockOnly": true
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

### Example 2: Track a repair job

**Query to AI Agent:**
```
"What's the status of repair job #42?"
```

**AI Agent calls:**
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

### Example 3: Get dashboard statistics

**Query to AI Agent:**
```
"Give me an overview of the system status"
```

**AI Agent calls:**
```json
{
  "tool": "GetDashboardStatistics"
}
```

**Response:**
```json
{
  "timestamp": "2026-06-29T22:00:00",
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

## Security & Best Practices

1. **Authentication**: MCP endpoint is exposed without authentication by default. In production, add authentication middleware.

2. **Rate Limiting**: Consider adding rate limiting to prevent abuse.

3. **Logging**: All MCP tool calls are logged with ILogger for audit trails.

4. **Error Handling**: All tools have comprehensive error handling and return user-friendly error messages.

5. **Input Validation**: All parameters are validated (clamped ranges, required fields checked).

6. **Read-Only Operations**: All exposed tools are read-only - no create, update, or delete operations for security.

---

## Technical Details

- **Protocol**: JSON-RPC 2.0 over HTTP/SSE
- **Transport**: HTTP with Server-Sent Events (Streamable HTTP)
- **Package**: ModelContextProtocol.AspNetCore 1.4.0
- **Framework**: .NET 10.0
- **Endpoint**: `/mcp`

## Troubleshooting

### MCP endpoint not responding
- Verify application is running on correct port
- Check that `app.MapMcp("/mcp")` is configured in Program.cs
- Ensure firewall allows HTTP traffic

### AI agent cannot discover tools
- Verify MCP server URL is correct
- Check client configuration (claude_desktop_config.json, etc.)
- Restart the AI client after configuration changes

### Empty or error responses
- Check application logs for errors
- Verify database connection is working
- Test endpoints with MCP Inspector: `npx @modelcontextprotocol/inspector`

---

## Future Enhancements

Potential improvements for production deployment:

1. **OAuth 2.0 Authentication** - Secure MCP endpoint with OAuth
2. **Write Operations** - Add create/update tools (with proper authorization)
3. **Webhooks** - Real-time notifications for status changes
4. **Batch Operations** - Process multiple requests in single call
5. **Caching** - Cache frequently requested data (product lists, statistics)
6. **Advanced Filtering** - More sophisticated query capabilities
7. **Export Tools** - Generate reports in PDF/Excel formats

---

## Support

For issues or questions about the MCP implementation:
- Check application logs in `logs/` directory
- Review MCP specification: https://modelcontextprotocol.io/
- Test with MCP Inspector for protocol-level debugging

---

**Last Updated:** 2026-06-29  
**Version:** 1.0.0  
**MCP Spec Version:** 2025-11-25
