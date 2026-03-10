using System.Net;
using System.Net.Http.Json;
using ExpenseManager.API.Models;
using FluentAssertions;

namespace ExpenseManager.API.Tests.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnTokens_WhenValid()
    {
        var request = new RegisterRequest("newuser@test.com", "Password1!", "John", "Doe");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrEmpty();
        auth.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldFail_WhenPasswordTooWeak()
    {
        var request = new RegisterRequest("weak@test.com", "123", "John", "Doe");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsValid()
    {
        // Register first
        var registerRequest = new RegisterRequest("login@test.com", "Password1!", "Jane", "Doe");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Login
        var loginRequest = new LoginRequest("login@test.com", "Password1!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenCredentialsInvalid()
    {
        var loginRequest = new LoginRequest("nobody@test.com", "WrongPass1!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
