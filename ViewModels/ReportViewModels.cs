using ExpenseManager.Models;

namespace ExpenseManager.ViewModels
{
    public class CategoryDetailsViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<Expense> Expenses { get; set; } = new List<Expense>();
    }

    public class MonthlyDetailsViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public List<Expense> Expenses { get; set; } = new List<Expense>();
    }
}