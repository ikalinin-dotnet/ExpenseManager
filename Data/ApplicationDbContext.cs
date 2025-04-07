// Data/ApplicationDbContext.cs
using ExpenseManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure many-to-one relationship between Expense and User
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
                
            // Configure many-to-one relationship between Expense and Approver
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Approver)
                .WithMany(u => u.ApprovedExpenses)
                .HasForeignKey(e => e.ApproverId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
                
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Travel", Description = "Travel expenses including airfare, hotel, car rental" },
                new Category { Id = 2, Name = "Meals", Description = "Business meals and entertainment" },
                new Category { Id = 3, Name = "Office Supplies", Description = "Stationary, small equipment, consumables" },
                new Category { Id = 4, Name = "Software", Description = "Software licenses and subscriptions" },
                new Category { Id = 5, Name = "Training", Description = "Courses, conferences, and educational materials" }
            );
        }
    }
}