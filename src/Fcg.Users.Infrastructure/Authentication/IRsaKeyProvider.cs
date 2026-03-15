using System.Security.Cryptography;

namespace Fcg.Users.Infrastructure.Authentication;

/// <summary>Provides RSA keys for JWT signing (private) and validation/JWKS (public). Supports key rotation: multiple public keys active, one current for signing.</summary>
public interface IRsaKeyProvider
{
    /// <summary>Key ID used when signing new tokens. Must be present in public keys returned by GetPublicKeysByKeyId.</summary>
    string CurrentKeyId { get; }

    /// <summary>Private key for the current key ID. Used only for signing; never exposed in JWKS.</summary>
    RSA GetPrivateKeyForSigning();

    /// <summary>All active public keys (current + previous for rotation). KeyId -> public key. Used for JWKS and local validation.</summary>
    IReadOnlyDictionary<string, RSA> GetPublicKeysByKeyId();
}
