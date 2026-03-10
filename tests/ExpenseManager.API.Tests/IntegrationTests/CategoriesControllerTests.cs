using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseManager.API.Models;
using ExpenseManager.Application.Categories.DTOs;
using FluentAssertions;

namespace ExpenseManager.API.Tests.IntegrationTests;

public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync(string email = "cat-user@test.com")
    {
        var registerRequest = new RegisterRequest(email, "Password1!", "Test", "User");
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
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
    public async Task GetAll_ShouldReturnSeededCategories()
    {
        var token = await GetTokenAsync("catlist@test.com");
        SetAuth(token);

        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
        categories!.Count.Should().BeGreaterThanOrEqualTo(6);
        categories.Should().Contain(c => c.Name == "Travel");
    }

    [Fact]
    public async Task GetById_ShouldReturnCategory_WhenExists()
    {
        var token = await GetTokenAsync("catget@test.com");
        SetAuth(token);

        var categoryId = Guid.Parse("d1f3e7a0-1234-4b8c-9a0e-000000000001");
        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category!.Name.Should().Be("Travel");
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotManager()
    {
        var token = await GetTokenAsync("catcreate-employee@test.com");
        SetAuth(token);

        var request = new CreateCategoryRequest("NewCategory", "Desc");
        var response = await _client.PostAsJsonAsync("/api/categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
