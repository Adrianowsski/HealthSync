using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Konfiguracja połączenia z bazą SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2️⃣ Konfiguracja Identity (User + Role) + EF store
builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// 3️⃣ Developer exception page dla EF
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 4️⃣ MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 5️⃣ Konfiguracja plików cookie, by używał naszych ścieżek:
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "HealthSync.Intranet.Cookie";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// 6️⃣ Middleware
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

// UWAGA: kolejność ważna!
app.UseAuthentication();
app.UseAuthorization();

// 7️⃣ Routing
app.MapControllerRoute(
    name: "chat",
    pattern: "Chat/{action=Index}/{appointmentId?}",
    defaults: new { controller = "Chat" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.MapRazorPages();

// 8️⃣ Seed roli i lekarza "doktor"
await SeedDoctorAsync(app.Services);

app.Run();


// ─── METODA SEEDUJĄCA ─────────────────────────────────────────────
static async Task SeedDoctorAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var db       = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Rola "Doctor"
    if (!await roleMgr.RoleExistsAsync("Doctor"))
        await roleMgr.CreateAsync(new IdentityRole("Doctor"));

    // Użytkownik "doktor"
    var user = await userMgr.FindByNameAsync("doktor");
    if (user == null)
    {
        user = new User
        {
            UserName = "doktor",
            Email    = "doktor@healthsync.com",
            EmailConfirmed = true
        };
        var res = await userMgr.CreateAsync(user, "Doktor123!");
        if (res.Succeeded)
            await userMgr.AddToRoleAsync(user, "Doctor");
    }

    // Profile lekarza
    if (!await db.DoctorProfiles.AnyAsync(p => p.UserId == user.Id))
    {
        db.DoctorProfiles.Add(new DoctorProfile
        {
            UserId        = user.Id,
            FirstName     = "Jan",
            LastName      = "Kowalski",
            Specialization = "General Medicine",
            Schedule      = "Mon-Fri 09:00-18:00"
        });
        await db.SaveChangesAsync();
    }
}
