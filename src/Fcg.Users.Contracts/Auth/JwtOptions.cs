namespace Fcg.Users.Contracts.Auth;

/// <summary>JWT configuration for RS256. Issuer must be a real URL for OIDC discovery and API Gateway JWT authorizer.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Issuer URL (e.g. https://users-api.example.com). Used in token "iss" and OIDC discovery.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Audience (e.g. fcg-cloud-platform). Used in token "aud" and validation.</summary>
    public string Audience { get; set; } = "fcg-cloud-platform";

    /// <summary>Access token lifetime in seconds.</summary>
    public int ExpirationSeconds { get; set; } = 3600;
}
