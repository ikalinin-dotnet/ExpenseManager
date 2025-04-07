using System;
using System.Collections.Generic;
using System.Globalization;

namespace ExpenseManager.ViewModels
{
    public class ExpenseSummaryViewModel
    {
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal RejectedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public List<CategorySummary> ExpensesByCategory { get; set; } = new List<CategorySummary>();
        public List<MonthSummary> ExpensesByMonth { get; set; } = new List<MonthSummary>();
    }

    public class CategorySummary
    {
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
    }

    public class MonthSummary
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
        public string MonthYear => $"{MonthName} {Year}";
    }
}