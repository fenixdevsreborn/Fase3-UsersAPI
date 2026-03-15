using System.Security.Cryptography;
using Fcg.Users.Contracts.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fcg.Users.Infrastructure.Authentication;

/// <summary>Loads RSA keys from PEM files. Development and simple production (keys on disk).</summary>
public sealed class FileRsaKeyProvider : IRsaKeyProvider
{
    private readonly Lazy<(RSA PrivateKey, IReadOnlyDictionary<string, RSA> PublicKeys)> _keys;
    private readonly string _currentKeyId;

    public FileRsaKeyProvider(IOptions<JwtSigningOptions> options, IHostEnvironment env)
    {
        var opts = options.Value;
        _currentKeyId = opts.CurrentKeyId;
        _keys = new Lazy<(RSA, IReadOnlyDictionary<string, RSA>)>(() => LoadKeys(opts.FilePath, opts.CurrentKeyId, env));
    }

    public string CurrentKeyId => _currentKeyId;

    public RSA GetPrivateKeyForSigning() => _keys.Value.PrivateKey;

    public IReadOnlyDictionary<string, RSA> GetPublicKeysByKeyId() => _keys.Value.PublicKeys;

    private static (RSA PrivateKey, IReadOnlyDictionary<string, RSA> PublicKeys) LoadKeys(string filePath, string currentKeyId, IHostEnvironment env)
    {
        var basePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(env.ContentRootPath ?? AppContext.BaseDirectory, filePath);
        string privateKeyPem;
        string keyId = currentKeyId;

        if (Directory.Exists(basePath))
        {
            var currentKeyIdPath = Path.Combine(basePath, "current-key-id.txt");
            if (File.Exists(currentKeyIdPath))
                keyId = File.ReadAllText(currentKeyIdPath).Trim();
            var privatePath = Path.Combine(basePath, keyId, "private.pem");
            if (!File.Exists(privatePath))
                throw new InvalidOperationException($"RSA private key not found at {privatePath}. Create keys directory with {keyId}/private.pem.");
            privateKeyPem = File.ReadAllText(privatePath);
        }
        else
        {
            if (!File.Exists(basePath))
            {
                if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
                    return GenerateAndPersistDevKeys(basePath, currentKeyId);
                throw new InvalidOperationException($"RSA private key file not found: {basePath}. Set Jwt:Signing:FilePath or run in Development to auto-generate.");
            }
            privateKeyPem = File.ReadAllText(basePath);
        }

        var privateKey = RSA.Create();
        privateKey.ImportFromPem(privateKeyPem);
        var publicKeys = new Dictionary<string, RSA> { [keyId] = ExportPublicOnly(privateKey) };
        return (privateKey, publicKeys);
    }

    private static (RSA PrivateKey, IReadOnlyDictionary<string, RSA> PublicKeys) GenerateAndPersistDevKeys(string basePath, string currentKeyId)
    {
        var dir = Path.GetDirectoryName(basePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var rsa = RSA.Create(2048);
        var privatePem = rsa.ExportRSAPrivateKeyPem();
        File.WriteAllText(basePath, privatePem);

        var privateKey = RSA.Create();
        privateKey.ImportFromPem(privatePem);
        var publicKeys = new Dictionary<string, RSA> { [currentKeyId] = ExportPublicOnly(privateKey) };
        return (privateKey, publicKeys);
    }

    private static RSA ExportPublicOnly(RSA rsa)
    {
        var pub = RSA.Create();
        pub.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);
        return pub;
    }
}
