using System.Security.Claims;
using Fcg.Users.Api.Authorization;
using Fcg.Users.Contracts.Auth;
using Xunit;

namespace Fcg.Users.UnitTests.Authorization;

public class UserClaimsExtensionsTests
{
    [Fact]
    public void GetUserId_WhenSubIsValidGuid_ReturnsGuid()
    {
        var id = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(FcgClaimTypes.Sub, id.ToString())
        }, "Test"));

        var result = principal.GetUserId();

        Assert.Equal(id, result);
    }

    [Fact]
    public void GetUserId_WhenSubMissing_ReturnsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "Test"));

        var result = principal.GetUserId();

        Assert.Null(result);
    }

    [Fact]
    public void GetRole_WhenRoleClaimPresent_ReturnsValue()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(FcgClaimTypes.Role, FcgRoles.Admin)
        }, "Test"));

        Assert.Equal(FcgRoles.Admin, principal.GetRole());
        Assert.True(principal.IsAdmin());
    }

    [Fact]
    public void IsAdmin_WhenUserRole_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(FcgClaimTypes.Role, FcgRoles.User)
        }, "Test"));

        Assert.False(principal.IsAdmin());
    }

    [Fact]
    public void GetScopes_WhenScopeClaimPresent_ReturnsSplitList()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(FcgClaimTypes.Scope, "games:read payments:write")
        }, "Test"));

        var scopes = principal.GetScopes();
        Assert.Equal(2, scopes.Count);
        Assert.True(principal.HasScope("games:read"));
        Assert.True(principal.HasScope("payments:write"));
        Assert.False(principal.HasScope("users:read"));
    }
}
