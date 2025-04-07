using ExpenseManager.Models;
using System.Collections.Generic;

namespace ExpenseManager.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Expense> PendingExpenses { get; set; } = new List<Expense>();
        public IEnumerable<Expense> UserExpenses { get; set; } = new List<Expense>();
        public int TotalExpenses { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}