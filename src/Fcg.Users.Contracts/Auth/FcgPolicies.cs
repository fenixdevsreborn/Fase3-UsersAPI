namespace Fcg.Users.Contracts.Auth;

/// <summary>Authorization policy names. Register with AddFcgAuthorization.</summary>
public static class FcgPolicies
{
    /// <summary>Any authenticated user.</summary>
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";

    /// <summary>User must have role admin.</summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>Policy name prefix for scope requirement; use RequireScopePolicyName(scope) for full name.</summary>
    public const string RequireScopePrefix = "RequireScope:";
}
