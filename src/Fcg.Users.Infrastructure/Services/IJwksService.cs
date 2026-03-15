namespace Fcg.Users.Infrastructure.Services;

/// <summary>Exposes JWKS (JSON Web Key Set) for token validation. Only public keys; used by /.well-known/jwks.json and API Gateway JWT authorizer.</summary>
public interface IJwksService
{
    /// <summary>Returns the JWKS document as JSON string. Contains all active public keys (current + rotation).</summary>
    string GetJwksJson();
}
