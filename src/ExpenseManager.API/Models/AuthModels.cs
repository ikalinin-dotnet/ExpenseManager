namespace ExpenseManager.API.Models;

/// <summary>Request model for user registration.</summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

/// <summary>Request model for user login.</summary>
public sealed record LoginRequest(
    string Email,
    string Password);

/// <summary>Request model for token refresh.</summary>
public sealed record RefreshTokenRequest(
    string Token,
    string RefreshToken);

/// <summary>Response model containing authentication tokens.</summary>
public sealed record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime Expiration);

/// <summary>Request model for creating an expense.</summary>
public sealed record CreateExpenseRequest(
    string Title,
    string? Description,
    decimal Amount,
    string Currency,
    DateTime Date,
    Guid CategoryId);

/// <summary>Request model for updating an expense.</summary>
public sealed record UpdateExpenseRequest(
    string Title,
    string? Description,
    decimal Amount,
    string Currency,
    DateTime Date,
    Guid CategoryId);

/// <summary>Request model for rejecting an expense.</summary>
public sealed record RejectExpenseRequest(string Reason);

/// <summary>Request model for creating a category.</summary>
public sealed record CreateCategoryRequest(string Name, string? Description);

/// <summary>Request model for updating a category.</summary>
public sealed record UpdateCategoryRequest(string Name, string? Description);
