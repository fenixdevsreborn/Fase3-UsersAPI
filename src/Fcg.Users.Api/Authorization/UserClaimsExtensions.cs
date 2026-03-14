using System.Security.Claims;
using Fcg.Users.Contracts.Auth;

namespace Fcg.Users.Api.Authorization;

/// <summary>Extensions to get FCG claims from ClaimsPrincipal. Use these instead of reading body/query for user identity.</summary>
public static class UserClaimsExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst(FcgClaimTypes.Sub)?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst(FcgClaimTypes.Email)?.Value;

    public static string? GetName(this ClaimsPrincipal user) =>
        user.FindFirst(FcgClaimTypes.Name)?.Value;

    public static string? GetRole(this ClaimsPrincipal user) =>
        user.FindFirst(FcgClaimTypes.Role)?.Value;

    public static IReadOnlyList<string> GetScopes(this ClaimsPrincipal user)
    {
        var scope = user.FindFirst(FcgClaimTypes.Scope)?.Value;
        if (string.IsNullOrWhiteSpace(scope)) return Array.Empty<string>();
        return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        string.Equals(user.GetRole(), FcgRoles.Admin, StringComparison.OrdinalIgnoreCase);

    public static bool HasScope(this ClaimsPrincipal user, string scope)
    {
        if (string.IsNullOrEmpty(scope)) return false;
        var scopes = user.GetScopes();
        return scopes.Any(s => string.Equals(s, scope, StringComparison.OrdinalIgnoreCase));
    }
}
