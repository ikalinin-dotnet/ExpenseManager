using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ExpenseManager.ViewModels;

namespace ExpenseManager.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }
        
        // GET: Reports/Summary
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
            
            // Calculate totals using in-memory LINQ and casting
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
                .GroupBy(e => e.Category.Name)
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

        // GET: Reports/CategoryDetails
        public async Task<IActionResult> CategoryDetails(string categoryName)
        {
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

        // GET: Reports/MonthlyDetails
        public async Task<IActionResult> MonthlyDetails(int year, int month)
        {
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