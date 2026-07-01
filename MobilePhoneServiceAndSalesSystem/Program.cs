using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.AspNetCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Infrastructure.Logging;
using MobilePhoneServiceAndSalesSystem.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var supportedCultures = new[]
{
    new CultureInfo("hr"),
    new CultureInfo("en-US")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

builder.Services.AddScoped<CrudActionLoggingFilter>();
builder.Services.AddScoped<MobilePhoneServiceAndSalesSystem.Infrastructure.AI.GroqAiService>();
builder.Services.AddHttpClient();

// MCP Server Configuration
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CrudActionLoggingFilter>();
});
builder.Services.AddRazorPages();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestingDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(
        builder.Configuration.GetConnectionString("AppDbContext"), new MySqlServerVersion(new Version(8, 0, 36))
    ));
}

builder.Services
    .AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, exception) =>
    {
        if (exception is not null || httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        return httpContext.Response.StatusCode >= 400
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
    };
});

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<UnhandledExceptionLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// MCP Endpoint - accessible at /mcp for AI agents
app.MapMcp("/mcp");

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

