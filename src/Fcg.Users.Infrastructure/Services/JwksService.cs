using System.Text.Json;
using Fcg.Users.Infrastructure.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Users.Infrastructure.Services;

/// <summary>Builds JWKS from IRsaKeyProvider. Exposes only public keys with kid, use=sig, alg=RS256.</summary>
public sealed class JwksService : IJwksService
{
    private readonly IRsaKeyProvider _keyProvider;

    public JwksService(IRsaKeyProvider keyProvider)
    {
        _keyProvider = keyProvider;
    }

    public string GetJwksJson()
    {
        var keyList = new List<object>();
        foreach (var (keyId, rsa) in _keyProvider.GetPublicKeysByKeyId())
        {
            var p = rsa.ExportParameters(false);
            var n = p.Modulus ?? Array.Empty<byte>();
            var e = p.Exponent ?? Array.Empty<byte>();
            if (n.Length == 0 || e.Length == 0) continue;
            keyList.Add(new
            {
                kty = "RSA",
                use = "sig",
                alg = "RS256",
                kid = keyId,
                n = Base64UrlEncoder.Encode(n),
                e = Base64UrlEncoder.Encode(e)
            });
        }
        var doc = new { keys = keyList };
        return JsonSerializer.Serialize(doc);
    }
}
