using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HealthSync.Shared.Data;
using HealthSync.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔌 Konfiguracja połączenia z SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🔐 Konfiguracja Identity (dla User + Role)
builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// 🔍 Developer helper (widoczne błędy bazy w trybie dev)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🖥️ Kontrolery + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 🔧 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // pokazuj błędy migracji
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // WAŻNE: musi być przed Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // dla loginu/rejestracji

app.Run();