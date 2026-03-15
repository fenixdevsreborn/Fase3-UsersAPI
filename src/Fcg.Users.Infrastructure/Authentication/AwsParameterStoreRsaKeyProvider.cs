using System.Security.Cryptography;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Fcg.Users.Contracts.Auth;
using Microsoft.Extensions.Options;

namespace Fcg.Users.Infrastructure.Authentication;

/// <summary>Loads RSA private key from AWS Systems Manager Parameter Store. Use for production; store private key PEM in SecureString parameter.</summary>
public sealed class AwsParameterStoreRsaKeyProvider : IRsaKeyProvider
{
    private readonly Lazy<(RSA PrivateKey, IReadOnlyDictionary<string, RSA> PublicKeys)> _keys;
    private readonly string _currentKeyId;

    public AwsParameterStoreRsaKeyProvider(
        IOptions<JwtSigningOptions> options,
        IAmazonSimpleSystemsManagement? ssm = null)
    {
        var opts = options.Value;
        _currentKeyId = opts.CurrentKeyId;
        var paramName = opts.AwsParameterName;
        var client = ssm ?? new AmazonSimpleSystemsManagementClient();
        _keys = new Lazy<(RSA, IReadOnlyDictionary<string, RSA>)>(() =>
        {
            var request = new GetParameterRequest
            {
                Name = paramName,
                WithDecryption = true
            };
            var response = client.GetParameterAsync(request).GetAwaiter().GetResult();
            var pem = response.Parameter.Value;
            if (string.IsNullOrWhiteSpace(pem))
                throw new InvalidOperationException($"SSM parameter {paramName} is empty.");
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
