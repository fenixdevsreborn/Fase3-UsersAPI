using System.Security.Cryptography;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Users.Api.Authentication;

/// <summary>Configures JwtBearerOptions with RS256 validation and key resolution from IRsaKeyProvider (by kid).</summary>
public sealed class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IRsaKeyProvider _keyProvider;
    private readonly ILogger<JwtBearerPostConfigureOptions> _logger;

    public JwtBearerPostConfigureOptions(
        IOptions<JwtOptions> jwtOptions,
        IRsaKeyProvider keyProvider,
        ILogger<JwtBearerPostConfigureOptions> logger)
    {
        _jwtOptions = jwtOptions;
        _keyProvider = keyProvider;
        _logger = logger;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var jwt = _jwtOptions.Value;
        var validIssuer = string.IsNullOrWhiteSpace(jwt.Issuer) ? null : jwt.Issuer.TrimEnd('/');
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = validIssuer,
            ValidAudience = jwt.Audience,
            ValidateIssuer = !string.IsNullOrEmpty(validIssuer),
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = FcgClaimTypes.Name,
            RoleClaimType = FcgClaimTypes.Role,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var publicKeys = _keyProvider.GetPublicKeysByKeyId();
                if (publicKeys.Count == 0) return null;
                if (!string.IsNullOrEmpty(kid) && publicKeys.TryGetValue(kid, out var rsa))
                    return new[] { new RsaSecurityKey(rsa) { KeyId = kid } };
                return publicKeys.Select(kv => new RsaSecurityKey(kv.Value) { KeyId = kv.Key }).ToArray();
            }
        };
        options.Events ??= new JwtBearerEvents();
        var existingFailed = options.Events.OnAuthenticationFailed;
        options.Events.OnAuthenticationFailed = async ctx =>
        {
            _logger.LogWarning(ctx.Exception, "JWT validation failed: {Message}", ctx.Exception.Message);
            if (existingFailed != null) await existingFailed(ctx);
        };
    }
}
