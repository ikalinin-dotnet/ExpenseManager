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

        /// <summary>
        /// Displays a list of expenses for the current user.
        /// Managers can see all expenses, while regular users see only their own.
        /// </summary>
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

        /// <summary>
        /// Displays the details of a specific expense.
        /// </summary>
        /// <param name="id">The ID of the expense to view</param>
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

        /// <summary>
        /// Displays the expense creation form.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        /// <summary>
        /// Processes the expense creation form submission.
        /// </summary>
        /// <param name="expense">The expense data from the form</param>
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
                    _context.Add(expense);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Expense created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the expense.");
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        /// <summary>
        /// Displays the expense edit form.
        /// </summary>
        /// <param name="id">The ID of the expense to edit</param>
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

        /// <summary>
        /// Processes the expense edit form submission.
        /// </summary>
        /// <param name="id">The ID of the expense to edit</param>
        /// <param name="expense">The updated expense data</param>
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

        /// <summary>
        /// Displays the expense deletion confirmation page.
        /// </summary>
        /// <param name="id">The ID of the expense to delete</param>
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

        /// <summary>
        /// Processes the expense deletion.
        /// </summary>
        /// <param name="id">The ID of the expense to delete</param>
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

        /// <summary>
        /// Displays a list of pending expenses for approval (manager only).
        /// </summary>
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

        /// <summary>
        /// Displays the expense approval confirmation page.
        /// </summary>
        /// <param name="id">The ID of the expense to approve</param>
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

        /// <summary>
        /// Processes the expense approval.
        /// </summary>
        /// <param name="id">The ID of the expense to approve</param>
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

        /// <summary>
        /// Displays the expense rejection form.
        /// </summary>
        /// <param name="id">The ID of the expense to reject</param>
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

        /// <summary>
        /// Processes the expense rejection.
        /// </summary>
        /// <param name="id">The ID of the expense to reject</param>
        /// <param name="rejectionReason">The reason for rejecting the expense</param>
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

        /// <summary>
        /// Checks if an expense with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID to check</param>
        /// <returns>True if the expense exists, otherwise false</returns>
        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}