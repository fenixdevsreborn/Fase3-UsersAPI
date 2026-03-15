using System.Security.Cryptography;
using Fcg.Users.Contracts.Auth;
using Microsoft.Extensions.Options;

namespace Fcg.Users.Infrastructure.Authentication;

/// <summary>Loads RSA private key from environment variable (e.g. container secret). Single key; CurrentKeyId from options.</summary>
public sealed class EnvironmentRsaKeyProvider : IRsaKeyProvider
{
    private readonly Lazy<(RSA PrivateKey, IReadOnlyDictionary<string, RSA> PublicKeys)> _keys;
    private readonly string _currentKeyId;

    public EnvironmentRsaKeyProvider(IOptions<JwtSigningOptions> options)
    {
        var opts = options.Value;
        _currentKeyId = opts.CurrentKeyId;
        var envVar = opts.EnvironmentVariableName;
        _keys = new Lazy<(RSA, IReadOnlyDictionary<string, RSA>)>(() =>
        {
            var pem = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrWhiteSpace(pem))
                throw new InvalidOperationException($"Environment variable {envVar} is not set. Set it to the RSA private key PEM for JWT signing.");
            var privateKey = RSA.Create();
            privateKey.ImportFromPem(pem);
            var publicKeys = new Dictionary<string, RSA> { [_currentKeyId] = ExportPublicOnly(privateKey) };
            return (privateKey, publicKeys);
        });
    }

    public string CurrentKeyId => _currentKeyId;

    public RSA GetPrivateKeyForSigning() => _keys.Value.PrivateKey;

    public IReadOnlyDictionary<string, RSA> GetPublicKeysByKeyId() => _keys.Value.PublicKeys;

    private static RSA ExportPublicOnly(RSA rsa)
    {
        var pub = RSA.Create();
        pub.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);
        return pub;
    }
}
