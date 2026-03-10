using ExpenseManager.Application.Categories.Commands.CreateCategory;
using ExpenseManager.Application.Categories.Commands.DeleteCategory;
using ExpenseManager.Application.Categories.Commands.UpdateCategory;
using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Categories.Queries.GetCategories;
using ExpenseManager.Application.Categories.Queries.GetCategoryById;
using ExpenseManager.API.Models;
using ExpenseManager.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

/// <summary>Manages expense categories.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all categories.</summary>
    /// <response code="200">Returns the list of categories.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCategoriesQuery());
        return Ok(result.Value);
    }

    /// <summary>Get a category by its ID.</summary>
    /// <param name="id">The category ID.</param>
    /// <response code="200">Returns the category.</response>
    /// <response code="404">Category not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new ProblemDetails { Detail = result.Error });
    }

    /// <summary>Create a new category. Requires Manager role.</summary>
    /// <param name="request">Category details.</param>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Invalid request data or duplicate name.</response>
    [HttpPost]
    [Authorize(Roles = ApplicationRole.Manager)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(request.Name, request.Description));

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new ProblemDetails { Detail = result.Error });
    }

    /// <summary>Update an existing category. Requires Manager role.</summary>
    /// <param name="id">The category ID.</param>
    /// <param name="request">Updated category details.</param>
    /// <response code="200">Category updated successfully.</response>
    /// <response code="400">Invalid request data or duplicate name.</response>
    /// <response code="404">Category not found.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationRole.Manager)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, request.Name, request.Description));

        if (result.IsSuccess) return Ok(result.Value);
        return result.Error!.Contains("not found")
            ? NotFound(new ProblemDetails { Detail = result.Error })
            : BadRequest(new ProblemDetails { Detail = result.Error });
    }

    /// <summary>Delete a category. Requires Manager role. Fails if expenses are associated.</summary>
    /// <param name="id">The category ID.</param>
    /// <response code="204">Category deleted successfully.</response>
    /// <response code="400">Category has associated expenses.</response>
    /// <response code="404">Category not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRole.Manager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id));

        if (result.IsSuccess) return NoContent();
        return result.Error!.Contains("not found")
            ? NotFound(new ProblemDetails { Detail = result.Error })
            : BadRequest(new ProblemDetails { Detail = result.Error });
    }
}
