using Microsoft.AspNetCore.Identity;

namespace ExpenseManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
        
        public virtual ICollection<Expense> Expenses { get; set; } = [];
        public virtual ICollection<Expense> ApprovedExpenses { get; set; } = [];
    }
}