using ExpenseManager.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Update connection string to use SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=ExpenseManager.db";

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity with Fluent API
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireManagerRole", policy =>
        policy.RequireRole("Manager"));
});

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed roles and admin user
await SeedData(app);

app.Run();

// Separate method for seeding data
static async Task SeedData(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Ensure the database is created and migrated
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Employee"))
            {
                await roleManager.CreateAsync(new IdentityRole("Employee"));
            }
            if (!await roleManager.RoleExistsAsync("Manager"))
            {
                await roleManager.CreateAsync(new IdentityRole("Manager"));
            }

            // Seed admin user if not exists
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Department = "Administration",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Manager");
                }
            }

            // Seed categories if they don't exist
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category 
                    { 
                        Name = "Travel", 
                        Description = "Travel expenses including airfare, hotel, car rental",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category 
                    { 
                        Name = "Meals", 
                        Description = "Business meals and entertainment",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category 
                    { 
                        Name = "Office Supplies", 
                        Description = "Stationary, small equipment, consumables",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category 
                    { 
                        Name = "Software", 
                        Description = "Software licenses and subscriptions",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category 
                    { 
                        Name = "Training", 
                        Description = "Courses, conferences, and educational materials",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw; // Re-throw to prevent application startup
        }
    }
}