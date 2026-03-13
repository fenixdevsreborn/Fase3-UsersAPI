using System.Net;
using System.Net.Http.Json;
using Fcg.Users.Contracts.Auth;
using Xunit;

namespace Fcg.Users.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<WebAppFixture>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebAppFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Email = "nobody@test.com",
            Password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidBootstrapAdmin_Returns200AndToken()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Email = "admin@fcg.local",
            Password = "ChangeMe@123"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.AccessToken);
        Assert.Equal("Bearer", body.TokenType);
        Assert.NotNull(body.User);
        Assert.Equal("admin", body.User.Role);
    }
}
