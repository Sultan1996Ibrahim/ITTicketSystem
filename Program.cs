using ITTicketSystem.Data;
using ITTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    "Host=localhost;Port=5432;Database=ITTicketSystem_V2;Username=itticket_v2;Password=StrongPass123";

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Enable in-memory cache and session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Enable session middleware
app.UseSession();

// UseAuthorization is recommended even if you don't use Identity
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ✅ Migrate + Seed (single scope)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Apply migrations
    db.Database.Migrate();

    // Seed departments
    SeedData.Initialize(scope.ServiceProvider);

    // Seed users + managers
    await UsersSeeder.SeedDefaultUsersAndManagersAsync(db);

    // Seed admin user if not exists
    var adminExists = await db.AppUsers.AnyAsync(u => u.UserName == "admin");
    if (!adminExists)
    {
        db.AppUsers.Add(new AppUser
        {
            UserName = "admin",
            PasswordHash = PasswordHelper.Hash("1234"),
            Role = UserRole.Admin,
            IsActive = true,
            DepartmentId = null
        });

        await db.SaveChangesAsync();
    }
}

app.Run();
