using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.Commands.ApproveExpense;
using ExpenseManager.Application.Expenses.Commands.CreateExpense;
using ExpenseManager.Application.Expenses.Commands.DeleteExpense;
using ExpenseManager.Application.Expenses.Commands.RejectExpense;
using ExpenseManager.Application.Expenses.Commands.SubmitExpense;
using ExpenseManager.Application.Expenses.Commands.UpdateExpense;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Application.Expenses.Queries.GetExpenseById;
using ExpenseManager.Application.Expenses.Queries.GetExpenses;
using ExpenseManager.API.Models;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

/// <summary>Manages expense operations including CRUD and workflow transitions.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get a paginated list of expenses with optional filters.</summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10).</param>
    /// <param name="userId">Filter by user ID.</param>
    /// <param name="status">Filter by expense status.</param>
    /// <param name="categoryId">Filter by category ID.</param>
    /// <response code="200">Returns the paginated list of expenses.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? userId = null,
        [FromQuery] ExpenseStatus? status = null,
        [FromQuery] Guid? categoryId = null)
    {
        var query = new GetExpensesQuery(pageNumber, pageSize, userId, status, categoryId);
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    /// <summary>Get an expense by its ID.</summary>
    /// <param name="id">The expense ID.</param>
    /// <response code="200">Returns the expense.</response>
    /// <response code="404">Expense not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetExpenseByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(ToProblem(result.Error!));
    }

    /// <summary>Create a new expense in Draft status.</summary>
    /// <param name="request">Expense details.</param>
    /// <response code="201">Expense created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var command = new CreateExpenseCommand(
            request.Title, request.Description, request.Amount,
            request.Currency, request.Date, request.CategoryId);

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(ToProblem(result.Error!));
    }

    /// <summary>Update an existing draft expense.</summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="request">Updated expense details.</param>
    /// <response code="200">Expense updated successfully.</response>
    /// <response code="400">Invalid request or business rule violation.</response>
    /// <response code="404">Expense not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseRequest request)
    {
        var command = new UpdateExpenseCommand(
            id, request.Title, request.Description, request.Amount,
            request.Currency, request.Date, request.CategoryId);

        var result = await _mediator.Send(command);

        if (result.IsSuccess) return Ok(result.Value);
        return result.Error!.Contains("not found") ? NotFound(ToProblem(result.Error)) : BadRequest(ToProblem(result.Error));
    }

    /// <summary>Delete a draft expense.</summary>
    /// <param name="id">The expense ID.</param>
    /// <response code="204">Expense deleted successfully.</response>
    /// <response code="400">Expense cannot be deleted.</response>
    /// <response code="404">Expense not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteExpenseCommand(id));

        if (result.IsSuccess) return NoContent();
        return result.Error!.Contains("not found") ? NotFound(ToProblem(result.Error)) : BadRequest(ToProblem(result.Error));
    }

    /// <summary>Submit a draft expense for approval.</summary>
    /// <param name="id">The expense ID.</param>
    /// <response code="204">Expense submitted successfully.</response>
    /// <response code="400">Expense cannot be submitted.</response>
    /// <response code="404">Expense not found.</response>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitExpenseCommand(id));

        if (result.IsSuccess) return NoContent();
        return result.Error!.Contains("not found") ? NotFound(ToProblem(result.Error)) : BadRequest(ToProblem(result.Error));
    }

    /// <summary>Approve a submitted expense. Requires Manager role.</summary>
    /// <param name="id">The expense ID.</param>
    /// <response code="204">Expense approved successfully.</response>
    /// <response code="400">Expense cannot be approved.</response>
    /// <response code="404">Expense not found.</response>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = ApplicationRole.Manager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveExpenseCommand(id));

        if (result.IsSuccess) return NoContent();
        return result.Error!.Contains("not found") ? NotFound(ToProblem(result.Error)) : BadRequest(ToProblem(result.Error));
    }

    /// <summary>Reject a submitted expense with a reason. Requires Manager role.</summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="request">Rejection details including reason.</param>
    /// <response code="204">Expense rejected successfully.</response>
    /// <response code="400">Expense cannot be rejected.</response>
    /// <response code="404">Expense not found.</response>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = ApplicationRole.Manager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseRequest request)
    {
        var result = await _mediator.Send(new RejectExpenseCommand(id, request.Reason));

        if (result.IsSuccess) return NoContent();
        return result.Error!.Contains("not found") ? NotFound(ToProblem(result.Error)) : BadRequest(ToProblem(result.Error));
    }

    private static ProblemDetails ToProblem(string detail) => new() { Detail = detail };
}
