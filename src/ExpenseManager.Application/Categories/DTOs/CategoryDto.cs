namespace ExpenseManager.Application.Categories.DTOs;

public sealed class CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}
