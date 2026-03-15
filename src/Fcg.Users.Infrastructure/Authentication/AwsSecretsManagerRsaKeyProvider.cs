using System.Security.Cryptography;
using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Fcg.Users.Contracts.Auth;
using Microsoft.Extensions.Options;

namespace Fcg.Users.Infrastructure.Authentication;

/// <summary>Loads RSA private key from AWS Secrets Manager. Secret can be JSON: { "keyId": "...", "privateKeyPem": "..." } or array for rotation.</summary>
public sealed class AwsSecretsManagerRsaKeyProvider : IRsaKeyProvider
{
    private readonly Lazy<(RSA PrivateKey, IReadOnlyDictionary<string, RSA> PublicKeys)> _keys;
    private readonly string _currentKeyId;

    public AwsSecretsManagerRsaKeyProvider(
        IOptions<JwtSigningOptions> options,
        IAmazonSecretsManager? secrets = null)
    {
        var opts = options.Value;
        _currentKeyId = opts.CurrentKeyId;
        var secretId = opts.AwsSecretId;
        var client = secrets ?? new AmazonSecretsManagerClient();
        _keys = new Lazy<(RSA, IReadOnlyDictionary<string, RSA>)>(() =>
        {
            var request = new GetSecretValueRequest { SecretId = secretId };
            var response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();
            var json = response.SecretString;
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException($"Secrets Manager secret {secretId} is empty.");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string keyId = opts.CurrentKeyId;
            string? privateKeyPem = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    var k = item.GetProperty("keyId").GetString();
                    var pem = item.GetProperty("privateKeyPem").GetString();
                    if (k == null || pem == null) continue;
                    if (k == opts.CurrentKeyId)
                    {
                        keyId = k;
                        privateKeyPem = pem;
                        break;
                    }
                    if (privateKeyPem == null) { keyId = k; privateKeyPem = pem; }
                }
            }
            else
            {
                keyId = root.GetProperty("keyId").GetString() ?? opts.CurrentKeyId;
                privateKeyPem = root.GetProperty("privateKeyPem").GetString();
            }

            if (string.IsNullOrWhiteSpace(privateKeyPem))
                throw new InvalidOperationException($"Secrets Manager secret {secretId} has no privateKeyPem for keyId {keyId}.");
            var privateKey = RSA.Create();
            privateKey.ImportFromPem(privateKeyPem);
            var publicKeys = new Dictionary<string, RSA> { [keyId] = ExportPublicOnly(privateKey) };
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
