using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ExpenseManager.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpensesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isManager = User.IsInRole("Manager");

            // For managers, show all expenses. For employees, only show their own
            var expenses = isManager
                ? _context.Expenses.Include(e => e.Category).Include(e => e.User)
                : _context.Expenses.Include(e => e.Category).Where(e => e.UserId == userId);

            var expenseList = await expenses.OrderByDescending(e => e.CreatedAt).ToListAsync();

            return View(expenseList);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Approver)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isManager = User.IsInRole("Manager");

            if (!isManager && expense.UserId != userId)
            {
                return Forbid();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Amount,Date,CategoryId")] Expense expense)
        {
            // Explicitly set the UserId from the currently logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Manually set properties not in the form
            expense.UserId = userId;
            expense.Status = ExpenseStatus.Pending;
            expense.CreatedAt = DateTime.Now;

            // Skip model validation for UserId since we're setting it manually
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("ModelState is valid, adding expense to context");
                    _context.Add(expense);

                    Console.WriteLine("Saving changes to database");
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Expense saved successfully!");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving expense: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while saving the expense.");
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        Console.WriteLine($"Error in {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId || expense.Status != ExpenseStatus.Pending)
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Amount,Date,CategoryId")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            var originalExpense = await _context.Expenses.FindAsync(id);
            if (originalExpense == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (originalExpense.UserId != userId || originalExpense.Status != ExpenseStatus.Pending)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    originalExpense.Title = expense.Title;
                    originalExpense.Description = expense.Description;
                    originalExpense.Amount = expense.Amount;
                    originalExpense.Date = expense.Date;
                    originalExpense.CategoryId = expense.CategoryId;
                    originalExpense.UpdatedAt = DateTime.UtcNow;

                    _context.Update(originalExpense);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Expense updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId || expense.Status != ExpenseStatus.Pending)
            {
                return Forbid();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId || expense.Status != ExpenseStatus.Pending)
            {
                return Forbid();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Expenses/Pending
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Pending()
        {
            var pendingExpenses = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .Where(e => e.Status == ExpenseStatus.Pending)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(pendingExpenses);
        }

        // GET: Expenses/Approve/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                return BadRequest("This expense is not pending approval.");
            }

            return View(expense);
        }

        // POST: Expenses/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                return BadRequest("This expense is not pending approval.");
            }

            expense.Status = ExpenseStatus.Approved;
            expense.ApproverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            expense.UpdatedAt = DateTime.UtcNow;

            _context.Update(expense);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense approved successfully!";
            return RedirectToAction(nameof(Pending));
        }

        // GET: Expenses/Reject/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                return BadRequest("This expense is not pending approval.");
            }

            return View(expense);
        }

        // POST: Expenses/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RejectConfirmed(int id, string rejectionReason)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                return BadRequest("This expense is not pending approval.");
            }

            expense.Status = ExpenseStatus.Rejected;
            expense.ApproverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            expense.RejectionReason = rejectionReason;
            expense.UpdatedAt = DateTime.UtcNow;

            _context.Update(expense);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense rejected successfully!";
            return RedirectToAction(nameof(Pending));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}