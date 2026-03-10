using ExpenseManager.Application.Reports.DTOs;
using ExpenseManager.Application.Reports.Queries.GetExpensesByCategory;
using ExpenseManager.Application.Reports.Queries.GetExpenseSummary;
using ExpenseManager.Application.Reports.Queries.GetMonthlyReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

/// <summary>Provides reporting endpoints for expense analytics.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get an expense summary with totals and status counts.</summary>
    /// <param name="userId">Optional user ID filter.</param>
    /// <response code="200">Returns the expense summary.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ExpenseSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] string? userId = null)
    {
        var result = await _mediator.Send(new GetExpenseSummaryQuery(userId));
        return Ok(result.Value);
    }

    /// <summary>Get expenses grouped by a specific category.</summary>
    /// <param name="categoryId">The category ID.</param>
    /// <response code="200">Returns the category expense report.</response>
    /// <response code="404">Category not found.</response>
    [HttpGet("by-category/{categoryId:guid}")]
    [ProducesResponseType(typeof(CategoryExpenseReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var result = await _mediator.Send(new GetExpensesByCategoryQuery(categoryId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new ProblemDetails { Detail = result.Error });
    }

    /// <summary>Get a monthly expense report grouped by category.</summary>
    /// <param name="year">Report year.</param>
    /// <param name="month">Report month (1-12).</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <response code="200">Returns the monthly report.</response>
    [HttpGet("monthly/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(MonthlyReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthly(int year, int month, [FromQuery] string? userId = null)
    {
        var result = await _mediator.Send(new GetMonthlyReportQuery(year, month, userId));
        return Ok(result.Value);
    }
}
