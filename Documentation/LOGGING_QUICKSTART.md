# Logging - Brzi Start

## Što je implementirano?

✅ **Serilog** - kompletno konfiguriran logging framework  
✅ **Automatsko logiranje HTTP zahtjeva** - sve zahtjeve prati Serilog  
✅ **Automatsko logiranje CRUD operacija** - CrudActionLoggingFilter  
✅ **Automatsko logiranje iznimki** - UnhandledExceptionLoggingMiddleware  

## Gdje se nalaze logovi?

```
logs/
  ├─ app-20260625.log      (Production)
  └─ dev-20260625.log      (Development)
```

## Kako koristiti logger u controlleru? (opcionalno)

```csharp
public class MyController : Controller
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    public IActionResult MyAction()
    {
        _logger.LogInformation("Custom message");
        _logger.LogWarning("Warning message");
        _logger.LogError(exception, "Error occurred");
        
        return View();
    }
}
```

## Log Levels

- **Debug**: Detaljne informacije (samo Development)
- **Information**: Opće informacije (HTTP zahtjevi, CRUD operacije)
- **Warning**: Upozorenja (4xx HTTP status kodovi)
- **Error**: Greške (iznimke, 5xx HTTP status kodovi)
- **Critical**: Kritične greške

## Primjer loga

```
2026-06-25 21:00:00.123 +02:00 [INF] HTTP POST /Phones/Create responded 302 in 67.8934 ms
2026-06-25 21:00:00.456 +02:00 [INF] CRUD action started: Phones.Create POST /Phones/Create User=admin@test.com
2026-06-25 21:00:00.789 +02:00 [INF] CRUD action completed: Phones.Create StatusCode=302 User=admin@test.com
```

## Konfiguracija

Sve postavke su u `appsettings.json` i `appsettings.Development.json` pod `Serilog` sekcijom.

Za više detalja pogledaj: [Infrastructure/Logging/README.md](README.md)
