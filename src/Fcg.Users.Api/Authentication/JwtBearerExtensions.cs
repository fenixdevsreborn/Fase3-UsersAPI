using System.Text;
using Fcg.Users.Contracts.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Users.Api.Authentication;

/// <summary>Adds FCG JWT Bearer authentication. Reads configuration from "Jwt" section. SigningKey must be at least 32 characters.</summary>
public static class JwtBearerExtensions
{
    public static IServiceCollection AddFcgJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);
        var signingKey = section["SigningKey"] ?? "";
        var issuer = section["Issuer"] ?? "Fcg.Users.Api";
        var audience = section["Audience"] ?? "fcg-cloud-platform";

        if (string.IsNullOrEmpty(signingKey) || signingKey.Length < JwtOptions.MinSigningKeyLength)
            throw new InvalidOperationException(
                $"Jwt:SigningKey must be set and at least {JwtOptions.MinSigningKeyLength} characters.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuerSigningKey = true,
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    NameClaimType = FcgClaimTypes.Name,
                    RoleClaimType = FcgClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        return services;
    }
}
