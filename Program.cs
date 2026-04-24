using ExamenII.Data;
using ExamenII.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ========================
// SERVICES
// ========================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=examenii.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

var app = builder.Build();

// ========================
// PIPELINE
// ========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ========================
// ROLES SEED
// ========================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!roleManager.RoleExistsAsync("Administrador").GetAwaiter().GetResult())
        roleManager.CreateAsync(new IdentityRole("Administrador")).GetAwaiter().GetResult();

    if (!roleManager.RoleExistsAsync("Usuario").GetAwaiter().GetResult())
        roleManager.CreateAsync(new IdentityRole("Usuario")).GetAwaiter().GetResult();
}

// ========================
// ADMIN USER SEED
// ========================
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string adminEmail = "admin@examen.com";
    string adminPassword = "Admin123!";

    var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();

    if (adminUser == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };

        var result = userManager.CreateAsync(admin, adminPassword).GetAwaiter().GetResult();

        if (result.Succeeded)
        {
            userManager.AddToRoleAsync(admin, "Administrador")
                .GetAwaiter().GetResult();
        }
    }
}

// ========================
// ROUTING
// ========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Evidencias}/{action=Index}/{id?}");

app.Run();