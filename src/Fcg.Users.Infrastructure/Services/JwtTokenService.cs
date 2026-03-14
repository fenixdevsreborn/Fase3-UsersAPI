using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Users.Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
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

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_options.ExpirationSeconds),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetExpirationSeconds() => _options.ExpirationSeconds;
}
