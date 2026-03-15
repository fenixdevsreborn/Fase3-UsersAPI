namespace Fcg.Users.Contracts.Auth;

/// <summary>Claim names used in FCG JWT. All consuming APIs must use these for validation (NameClaimType, RoleClaimType) and helpers.</summary>
public static class FcgClaimTypes
{
    /// <summary>Subject — user id (GUID).</summary>
    public const string Sub = "sub";

    /// <summary>Email address.</summary>
    public const string Email = "email";

    /// <summary>Username (unique, case-insensitive).</summary>
    public const string Username = "username";

    /// <summary>Display name.</summary>
    public const string Name = "name";

    /// <summary>Role: <see cref="FcgRoles.Admin"/> or <see cref="FcgRoles.User"/>.</summary>
    public const string Role = "role";

    /// <summary>Space-separated scopes (e.g. "games:read payments:write").</summary>
    public const string Scope = "scope";

    /// <summary>JWT ID — unique token id for revocation/audit.</summary>
    public const string Jti = "jti";
}
