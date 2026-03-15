namespace Fcg.Users.Contracts.Auth;

/// <summary>Configuration for RSA key loading and rotation. Private key is never exposed; only public keys appear in JWKS.</summary>
public class JwtSigningOptions
{
    public const string SectionName = "Jwt:Signing";

    /// <summary>Provider: File, Environment, AwsParameterStore, AwsSecretsManager.</summary>
    public string Provider { get; set; } = "File";

    /// <summary>Current key ID used for signing new tokens (e.g. "2024-01", "dev-1"). Included as "kid" in JWT header.</summary>
    public string CurrentKeyId { get; set; } = "default";

    /// <summary>For File provider: path to PEM file (private key) or directory containing keyId/private.pem for rotation.</summary>
    public string FilePath { get; set; } = "keys/private.pem";

    /// <summary>For Environment provider: env var name containing PEM (e.g. JWT_PRIVATE_KEY_PEM).</summary>
    public string EnvironmentVariableName { get; set; } = "JWT_PRIVATE_KEY_PEM";

    /// <summary>For AwsParameterStore: parameter name for current private key PEM, or prefix for /keys/{keyId}/private.</summary>
    public string AwsParameterName { get; set; } = "/fcg/jwt/private-key";

    /// <summary>For AwsSecretsManager: secret ID or ARN containing JSON { "keyId": "...", "privateKeyPem": "..." } or array for rotation.</summary>
    public string AwsSecretId { get; set; } = "fcg/jwt/signing-keys";
}
