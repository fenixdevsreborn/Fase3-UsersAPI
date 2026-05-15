using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ms_users.Services;
using System.Security.Claims;

namespace ms_users.Tests;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_WithValidUser_ReturnsTokenThatCanBeValidated()
    {
        var service = CreateJwtService();

        var token = service.GenerateToken("user-123", "player@test.com");

        Assert.False(string.IsNullOrWhiteSpace(token));

        var principal = service.ValidateToken(token);

        Assert.NotNull(principal);
        var userIdClaim = principal.FindFirst("sub")
            ?? principal.FindFirst(ClaimTypes.NameIdentifier);

        Assert.Equal("user-123", userIdClaim?.Value);
        var emailClaim = principal.FindFirst("email")
            ?? principal.FindFirst(ClaimTypes.Email);

        Assert.Equal("player@test.com", emailClaim?.Value);
        Assert.Equal("user-123", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("player@test.com", principal.FindFirst(ClaimTypes.Email)?.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        var service = CreateJwtService();

        var principal = service.ValidateToken("not-a-valid-jwt");

        Assert.Null(principal);
    }

    [Fact]
    public void Constructor_WhenJwtSecretIsMissing_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "ms-users-api",
                ["Jwt:Audience"] = "fase4-apis"
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            new JwtService(configuration, NullLogger<JwtService>.Instance));
    }

    private static JwtService CreateJwtService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "this-is-a-test-secret-with-more-than-32-chars",
                ["Jwt:Issuer"] = "ms-users-api",
                ["Jwt:Audience"] = "fase4-apis",
                ["Jwt:KeyId"] = "ms-users-api-signing-key"
            })
            .Build();

        return new JwtService(configuration, NullLogger<JwtService>.Instance);
    }
}
