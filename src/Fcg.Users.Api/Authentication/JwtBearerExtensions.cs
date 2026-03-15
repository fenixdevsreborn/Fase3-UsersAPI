using Fcg.Users.Contracts.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fcg.Users.Api.Authentication;

/// <summary>Adds FCG JWT Bearer authentication with RS256 and key resolution from IRsaKeyProvider. Issuer must be URL for OIDC.</summary>
public static class JwtBearerExtensions
{
    public static IServiceCollection AddFcgJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }
}
