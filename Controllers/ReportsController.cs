using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ExpenseManager.ViewModels;

namespace ExpenseManager.Controllers
{
    /// <summary>
    /// Controller for expense reporting functionality.
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the reports index page with available report types.
        /// </summary>
        /// <returns>The reports index view.</returns>
        public IActionResult Index()
        {
            return View();
        }
        
        /// <summary>
        /// Displays a summary of expenses grouped by category and month.
        /// </summary>
        /// <returns>The summary view with expense data.</returns>
        public async Task<IActionResult> Summary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isManager = User.IsInRole("Manager");
            
            var viewModel = new ExpenseSummaryViewModel();
            
            // Fetch all expenses based on role
            var expensesQuery = isManager 
                ? _context.Expenses.Include(e => e.Category)
                : _context.Expenses.Where(e => e.UserId == userId).Include(e => e.Category);

            var expenses = await expensesQuery.ToListAsync();
            
            // Calculate totals using in-memory LINQ
            viewModel.TotalAmount = expenses.Sum(e => e.Amount);
            viewModel.ApprovedAmount = expenses
                .Where(e => e.Status == ExpenseStatus.Approved)
                .Sum(e => e.Amount);
            viewModel.RejectedAmount = expenses
                .Where(e => e.Status == ExpenseStatus.Rejected)
                .Sum(e => e.Amount);
            viewModel.PendingAmount = expenses
                .Where(e => e.Status == ExpenseStatus.Pending)
                .Sum(e => e.Amount);
                
            // Calculate expenses by category
            viewModel.ExpensesByCategory = expenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .Select(g => new CategorySummary 
                { 
                    CategoryName = g.Key, 
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();
                
            // Calculate expenses by month
            viewModel.ExpensesByMonth = expenses
                .GroupBy(e => new { Month = e.Date.Month, Year = e.Date.Year })
                .Select(g => new MonthSummary 
                { 
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();
            
            return View(viewModel);
        }

        /// <summary>
        /// Displays detailed information about expenses in a specific category.
        /// </summary>
        /// <param name="categoryName">The name of the category to view details for.</param>
        /// <returns>The category details view with related expenses.</returns>
        public async Task<IActionResult> CategoryDetails(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return BadRequest("Category name is required");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isManager = User.IsInRole("Manager");

            var query = isManager
                ? _context.Expenses.Include(e => e.Category).Include(e => e.User)
                : _context.Expenses.Where(e => e.UserId == userId)
                    .Include(e => e.Category)
                    .Include(e => e.User);

            var categoryExpenses = await query
                .Where(e => e.Category.Name == categoryName)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var viewModel = new CategoryDetailsViewModel
            {
                CategoryName = categoryName,
                Expenses = categoryExpenses,
                TotalAmount = categoryExpenses.Sum(e => e.Amount)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays detailed information about expenses in a specific month and year.
        /// </summary>
        /// <param name="year">The year to filter expenses by.</param>
        /// <param name="month">The month to filter expenses by (1-12).</param>
        /// <returns>The monthly details view with related expenses.</returns>
        public async Task<IActionResult> MonthlyDetails(int year, int month)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest("Month must be between 1 and 12");
            }

            if (year < 2000 || year > 2100)
            {
                return BadRequest("Year must be between 2000 and 2100");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isManager = User.IsInRole("Manager");

            var query = isManager
                ? _context.Expenses.Include(e => e.Category).Include(e => e.User)
                : _context.Expenses.Where(e => e.UserId == userId)
                    .Include(e => e.Category)
                    .Include(e => e.User);

            var monthExpenses = await query
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var viewModel = new MonthlyDetailsViewModel
            {
                Year = year,
                Month = month,
                Expenses = monthExpenses,
                TotalAmount = monthExpenses.Sum(e => e.Amount)
            };

            return View(viewModel);
        }
    }
}