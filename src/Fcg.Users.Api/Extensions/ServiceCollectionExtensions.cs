using Fcg.Users.Api.Authentication;
using Fcg.Users.Api.Authorization;
using Fcg.Users.Api.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Users.Api.Extensions;

/// <summary>Combined DI extensions for Users API: JWT, Authorization, Observability.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds FCG JWT Bearer authentication and authorization (RequireAuthenticatedUser, RequireAdmin, scope policies).</summary>
    public static IServiceCollection AddUsersApiAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFcgJwtBearer(configuration);
        services.AddFcgAuthorization();
        return services;
    }

    /// <summary>Adds observability: ActivitySource, FcgMeters, correlation/trace accessor, and option to add middlewares via UseFcgObservability.</summary>
    public static IServiceCollection AddUsersApiObservability(this IServiceCollection services, IConfiguration configuration, string projectName = "Fcg.Users.Api")
    {
        services.AddProjectObservability(configuration, projectName);
        return services;
    }
}
