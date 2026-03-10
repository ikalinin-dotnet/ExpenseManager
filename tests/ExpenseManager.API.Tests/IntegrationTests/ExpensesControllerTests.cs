using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseManager.API.Models;
using ExpenseManager.Application.Expenses.DTOs;
using FluentAssertions;

namespace ExpenseManager.API.Tests.IntegrationTests;

public class ExpensesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpensesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync(string email = "expense-user@test.com")
    {
        var registerRequest = new RegisterRequest(email, "Password1!", "Test", "User");
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // Already registered, login instead
            var loginRequest = new LoginRequest(email, "Password1!");
            response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    private void SetAuth(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WhenNotAuthenticated()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/expenses");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenValid()
    {
        var token = await GetTokenAsync("create-expense@test.com");
        SetAuth(token);

        // Use a seeded category ID
        var categoryId = Guid.Parse("d1f3e7a0-1234-4b8c-9a0e-000000000001");
        var request = new CreateExpenseRequest("Hotel", "Business hotel", 150m, "USD", DateTime.UtcNow, categoryId);

        var response = await _client.PostAsJsonAsync("/api/expenses", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var expense = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        expense.Should().NotBeNull();
        expense!.Title.Should().Be("Hotel");
        expense.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task GetById_ShouldReturnExpense_WhenExists()
    {
        var token = await GetTokenAsync("getbyid-expense@test.com");
        SetAuth(token);

        var categoryId = Guid.Parse("d1f3e7a0-1234-4b8c-9a0e-000000000001");
        var createRequest = new CreateExpenseRequest("Flight", null, 500m, "USD", DateTime.UtcNow, categoryId);
        var createResponse = await _client.PostAsJsonAsync("/api/expenses", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        var response = await _client.GetAsync($"/api/expenses/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var expense = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        expense!.Title.Should().Be("Flight");
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenNotExists()
    {
        var token = await GetTokenAsync("getbyid404@test.com");
        SetAuth(token);

        var response = await _client.GetAsync($"/api/expenses/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Submit_ShouldReturn204_WhenValid()
    {
        var token = await GetTokenAsync("submit-expense@test.com");
        SetAuth(token);

        var categoryId = Guid.Parse("d1f3e7a0-1234-4b8c-9a0e-000000000002");
        var createRequest = new CreateExpenseRequest("Lunch", null, 30m, "USD", DateTime.UtcNow, categoryId);
        var createResponse = await _client.PostAsJsonAsync("/api/expenses", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        var response = await _client.PostAsync($"/api/expenses/{created!.Id}/submit", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_ForDraftExpense()
    {
        var token = await GetTokenAsync("delete-expense@test.com");
        SetAuth(token);

        var categoryId = Guid.Parse("d1f3e7a0-1234-4b8c-9a0e-000000000003");
        var createRequest = new CreateExpenseRequest("Pen", null, 5m, "USD", DateTime.UtcNow, categoryId);
        var createResponse = await _client.PostAsJsonAsync("/api/expenses", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        var response = await _client.DeleteAsync($"/api/expenses/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
