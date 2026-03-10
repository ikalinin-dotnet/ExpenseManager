using ExpenseManager.Infrastructure.Data;
using ExpenseManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseManager.API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Remove the interceptor registration that depends on IDateTimeProvider
            var interceptorDescriptor = services.SingleOrDefault(
                d => d.ImplementationType?.Name == "AuditableEntitySaveChangesInterceptor");
            if (interceptorDescriptor is not null)
                services.Remove(interceptorDescriptor);

            // Use SQLite in-memory for tests
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Build the service provider and initialize the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // Seed roles
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!roleManager.RoleExistsAsync(ApplicationRole.Employee).GetAwaiter().GetResult())
                roleManager.CreateAsync(new IdentityRole(ApplicationRole.Employee)).GetAwaiter().GetResult();
            if (!roleManager.RoleExistsAsync(ApplicationRole.Manager).GetAwaiter().GetResult())
                roleManager.CreateAsync(new IdentityRole(ApplicationRole.Manager)).GetAwaiter().GetResult();
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
