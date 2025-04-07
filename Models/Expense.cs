using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Required(ErrorMessage = "User is required")]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public string? ApproverId { get; set; }

        public ApplicationUser? Approver { get; set; }

        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [StringLength(500, ErrorMessage = "Rejection reason cannot be longer than 500 characters")]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public enum ExpenseStatus
    {
        Pending,
        Approved,
        Rejected
    }
}