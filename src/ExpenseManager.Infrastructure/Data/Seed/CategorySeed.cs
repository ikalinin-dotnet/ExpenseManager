using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Data.Seed;

public static class CategorySeed
{
    public static void SeedCategories(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            CreateCategory("d1f3e7a0-1234-4b8c-9a0e-000000000001", "Travel", "Business travel expenses"),
            CreateCategory("d1f3e7a0-1234-4b8c-9a0e-000000000002", "Meals", "Food and dining expenses"),
            CreateCategory("d1f3e7a0-1234-4b8c-9a0e-000000000003", "Office Supplies", "Office equipment and supplies"),
            CreateCategory("d1f3e7a0-1234-4b8c-9a0e-000000000004", "Software", "Software licenses and subscriptions"),
            CreateCategory("d1f3e7a0-1234-4b8c-9a0e-000000000005", "Training", "Courses, conferences, and training"),
            CreateCategory("d1f3e7a0-1234-4b8c-9a0e-000000000006", "Other", "Miscellaneous expenses")
        );
    }

    private static object CreateCategory(string id, string name, string description) => new
    {
        Id = Guid.Parse(id),
        Name = name,
        Description = description,
        CreatedAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };
}
