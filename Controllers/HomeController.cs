using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using ExpenseManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ExpenseManager.ViewModels;

namespace ExpenseManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
{
    // Only show dashboard data for authenticated users
    if (User.Identity.IsAuthenticated)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"Dashboard for user ID: {userId}");
        
        var isManager = User.IsInRole("Manager");
        
        var viewModel = new DashboardViewModel();
        
        if (isManager)
        {
            // Manager dashboard code...
        }
        else
        {
            // For employees, show their own recent expenses
            var userExpenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .ToListAsync();
                
            viewModel.UserExpenses = userExpenses;
            
            // Show counts for user's expenses
            viewModel.TotalExpenses = await _context.Expenses.CountAsync(e => e.UserId == userId);
            viewModel.PendingCount = await _context.Expenses.CountAsync(e => e.UserId == userId && e.Status == ExpenseStatus.Pending);
            viewModel.ApprovedCount = await _context.Expenses.CountAsync(e => e.UserId == userId && e.Status == ExpenseStatus.Approved);
            viewModel.RejectedCount = await _context.Expenses.CountAsync(e => e.UserId == userId && e.Status == ExpenseStatus.Rejected);
        }
        
        return View(viewModel);
    }
    
    return View();
}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}