using System.Globalization;
using Microsoft.AspNetCore.Localization;
using MobilePhoneServiceAndSalesSystem.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var defaultCulture = new CultureInfo("en-IE");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(
    builder.Configuration.GetConnectionString("AppDbContext"), new MySqlServerVersion(new Version(8, 0, 36))
));

var app = builder.Build();

app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
