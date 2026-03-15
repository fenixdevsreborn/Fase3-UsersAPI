using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Users.Infrastructure.Services;

/// <summary>Generates JWT access tokens with RS256 and kid header. Uses IRsaKeyProvider for signing key.</summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IRsaKeyProvider _keyProvider;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, IRsaKeyProvider keyProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _keyProvider = keyProvider;
    }

    public string GenerateAccessToken(User user)
    {
        var roleValue = user.Role == UserRole.Admin ? FcgRoles.Admin : FcgRoles.User;
        var scopeValue = user.Role == UserRole.Admin
            ? "admin users:write users:read games:read games:write payments:read"
            : "games:read payments:read payments:write users:write:me";

        var claims = new List<Claim>
        {
            new(FcgClaimTypes.Sub, user.Id.ToString()),
            new(FcgClaimTypes.Email, user.Email),
            new(FcgClaimTypes.Name, user.Name ?? string.Empty),
            new(FcgClaimTypes.Role, roleValue),
            new(FcgClaimTypes.Scope, scopeValue),
            new(FcgClaimTypes.Jti, Guid.NewGuid().ToString())
        };

        var privateKey = _keyProvider.GetPrivateKeyForSigning();
        var securityKey = new RsaSecurityKey(privateKey) { KeyId = _keyProvider.CurrentKeyId };
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_jwtOptions.ExpirationSeconds),
            notBefore: DateTime.UtcNow,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetExpirationSeconds() => _jwtOptions.ExpirationSeconds;
}
