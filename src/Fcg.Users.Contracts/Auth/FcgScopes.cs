namespace Fcg.Users.Contracts.Auth;

/// <summary>Scope values for FCG APIs. Used for scope-based authorization.</summary>
public static class FcgScopes
{
    public const string Admin = "admin";
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersWriteMe = "users:write:me";
    public const string GamesRead = "games:read";
    public const string GamesWrite = "games:write";
    public const string PaymentsRead = "payments:read";
    public const string PaymentsWrite = "payments:write";
}
