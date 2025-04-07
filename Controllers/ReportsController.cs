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
            
            if (isManager)
            {
                // For managers, show summary of all expenses
                viewModel.TotalAmount = await _context.Expenses.SumAsync(e => e.Amount);
                viewModel.ApprovedAmount = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Approved)
                    .SumAsync(e => e.Amount);
                viewModel.RejectedAmount = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Rejected)
                    .SumAsync(e => e.Amount);
                viewModel.PendingAmount = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Pending)
                    .SumAsync(e => e.Amount);
                    
                // Calculate expenses by category
                viewModel.ExpensesByCategory = await _context.Expenses
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category.Name)
                    .Select(g => new CategorySummary 
                    { 
                        CategoryName = g.Key, 
                        Amount = g.Sum(e => e.Amount) 
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToListAsync();
                    
                // Calculate expenses by month
                viewModel.ExpensesByMonth = await _context.Expenses
                    .GroupBy(e => new { Month = e.Date.Month, Year = e.Date.Year })
                    .Select(g => new MonthSummary 
                    { 
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Amount = g.Sum(e => e.Amount) 
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();
            }
            else
            {
                // For employees, show summary of only their expenses
                viewModel.TotalAmount = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .SumAsync(e => e.Amount);
                viewModel.ApprovedAmount = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Status == ExpenseStatus.Approved)
                    .SumAsync(e => e.Amount);
                viewModel.RejectedAmount = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Status == ExpenseStatus.Rejected)
                    .SumAsync(e => e.Amount);
                viewModel.PendingAmount = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Status == ExpenseStatus.Pending)
                    .SumAsync(e => e.Amount);
                    
                // Calculate expenses by category
                viewModel.ExpensesByCategory = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category.Name)
                    .Select(g => new CategorySummary 
                    { 
                        CategoryName = g.Key, 
                        Amount = g.Sum(e => e.Amount) 
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToListAsync();
                    
                // Calculate expenses by month
                viewModel.ExpensesByMonth = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .GroupBy(e => new { Month = e.Date.Month, Year = e.Date.Year })
                    .Select(g => new MonthSummary 
                    { 
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Amount = g.Sum(e => e.Amount) 
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();
            }
            
            return View(viewModel);
        }
    }
}