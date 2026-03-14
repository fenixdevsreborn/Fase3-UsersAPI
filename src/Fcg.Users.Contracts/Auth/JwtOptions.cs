namespace Fcg.Users.Contracts.Auth;

/// <summary>JWT configuration. Used by Users API for issuance and validation; same section for all FCG APIs.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Fcg.Users.Api";
    public string Audience { get; set; } = "fcg-cloud-platform";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationSeconds { get; set; } = 3600;

    /// <summary>Minimum length for SigningKey (security requirement).</summary>
    public const int MinSigningKeyLength = 32;
}
