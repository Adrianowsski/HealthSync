using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HealthSync.Shared.Data;
using HealthSync.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// 💾 Połączenie z bazą danych SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 👤 Konfiguracja Identity z naszym modelem User i rolami
builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        // opcjonalnie: możesz dopisać reguły walidacji hasła tutaj
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// 🛠 Strona błędów EF + Razor Pages + MVC
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 🔧 Middleware i konfiguracja aplikacji
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Uwierzytelnianie musi być przed autoryzacją
app.UseAuthorization();

// 🔗 Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// 📌 Inicjalizacja roli "Patient"
await SeedRolesAsync(app.Services);

app.Run();

// 🎯 Metoda pomocnicza: Tworzenie roli "Patient" jeśli nie istnieje
async Task SeedRolesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Patient"))
    {
        await roleManager.CreateAsync(new IdentityRole("Patient"));
    }
}
// 🗝️ Konfiguracja plików cookie dla Portalu
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "HealthSync.Portal.Cookie";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
