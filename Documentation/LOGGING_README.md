# Logging Mehanizam

## Pregled

Projekt koristi **Serilog** kao logging framework. Implementiran je robustan logging mehanizam koji bilježi:
- HTTP zahtjeve i odgovore
- CRUD operacije
- Neobrađene iznimke (unhandled exceptions)

## Komponente

### 1. Serilog (glavni logging framework)
- **Paketi**: `Serilog.AspNetCore`, `Serilog.Sinks.File`
- **Konfiguracija**: `appsettings.json` i `appsettings.Development.json`
- **Outputi**: Console i File

### 2. CrudActionLoggingFilter
Bilježi sve CRUD operacije (Create, Edit, Delete, Post, Put).

**Logira:**
- Controller i action naziv
- HTTP metodu i path
- Korisničko ime
- Route ID (ako postoji)
- StatusCode nakon izvršenja
- Iznimke ako se pojave

### 3. UnhandledExceptionLoggingMiddleware
Hvata sve neobrađene iznimke i logira ih prije nego se propagiraju dalje.

**Logira:**
- Exception tip i message
- HTTP metodu i path
- Korisničko ime

## Konfiguracija

### Production (`appsettings.json`)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 14
        }
      }
    ]
  }
}
```

### Development (`appsettings.Development.json`)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/dev-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

## Lokacija log datoteka

- Production: `logs/app-YYYYMMDD.log`
- Development: `logs/dev-YYYYMMDD.log`

Log datoteke se automatski rotiraju dnevno:
- Production: zadržava 14 dana
- Development: zadržava 7 dana

## Primjeri log poruka

### HTTP Request
```
2026-06-25 21:00:00.123 +02:00 [INF] HTTP GET /Phones responded 200 in 45.2341 ms
```

### CRUD Action
```
2026-06-25 21:00:00.456 +02:00 [INF] CRUD action started: Phones.Create POST /Phones/Create User=admin@test.com RouteId=null
2026-06-25 21:00:00.789 +02:00 [INF] CRUD action completed: Phones.Create POST /Phones/Create StatusCode=302 User=admin@test.com RouteId=null
```

### Unhandled Exception
```
2026-06-25 21:00:00.999 +02:00 [ERR] Unhandled exception for POST /Phones/Create User=admin@test.com
System.InvalidOperationException: Database connection failed
   at ...
```
