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

        /// <summary>
        /// Displays the home page with a dashboard for authenticated users.
        /// </summary>
        /// <returns>The home page view with dashboard data for authenticated users or a welcome page for guests.</returns>
        public async Task<IActionResult> Index()
        {
            // Only show dashboard data for authenticated users
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isManager = User.IsInRole("Manager");
                var viewModel = new DashboardViewModel();

                if (isManager)
                {
                    // For managers, show pending expenses that need approval
                    viewModel.PendingExpenses = await _context.Expenses
                        .Include(e => e.User)
                        .Include(e => e.Category)
                        .Where(e => e.Status == ExpenseStatus.Pending)
                        .OrderByDescending(e => e.CreatedAt)
                        .Take(5)
                        .ToListAsync();

                    // Show counts for all expenses
                    viewModel.TotalExpenses = await _context.Expenses.CountAsync();
                    viewModel.PendingCount = await _context.Expenses.CountAsync(e => e.Status == ExpenseStatus.Pending);
                    viewModel.ApprovedCount = await _context.Expenses.CountAsync(e => e.Status == ExpenseStatus.Approved);
                    viewModel.RejectedCount = await _context.Expenses.CountAsync(e => e.Status == ExpenseStatus.Rejected);
                }
                else
                {
                    // For employees, show their own recent expenses
                    viewModel.UserExpenses = await _context.Expenses
                        .Include(e => e.Category)
                        .Where(e => e.UserId == userId)
                        .OrderByDescending(e => e.CreatedAt)
                        .Take(5)
                        .ToListAsync();

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

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>The privacy policy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays an error page when an error occurs in the application.
        /// </summary>
        /// <returns>The error view with request ID information.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}