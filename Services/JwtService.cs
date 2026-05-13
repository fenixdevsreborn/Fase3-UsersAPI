using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ms_users.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string email);
    ClaimsPrincipal ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var jwtSecret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        {
            KeyId = _configuration["Jwt:KeyId"] ?? "ms-users-api-signing-key"
        };
    }

    public string GenerateToken(string userId, string email)
    {
        _logger.LogInformation("Generating token for userId: {UserId}, email: {Email}", userId, email);

        var claims = new List<Claim>
        {
            new Claim("sub", userId),  // IMPORTANTE: sub claim
            new Claim("email", email),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim("exp", DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds().ToString())
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Token generated successfully");

        return tokenString;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) => new[] { _key },
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "sub",
                RoleClaimType = "role"
            }, out SecurityToken validatedToken);

            _logger.LogInformation("Token validated successfully");
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError("Token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
