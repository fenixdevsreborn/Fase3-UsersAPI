using Fcg.Users.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Users.Api.Authorization;

/// <summary>Registers FCG authorization policies: RequireAuthenticatedUser, RequireAdmin, and optional RequireScope.</summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddFcgAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(FcgPolicies.RequireAuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(FcgPolicies.RequireAdmin, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(FcgRoles.Admin));
        });
        return services;
    }

    /// <summary>Returns policy name for requiring a specific scope (e.g. "RequireScope:payments:write").</summary>
    public static string RequireScopePolicyName(string scope) => FcgPolicies.RequireScopePrefix + scope;

    /// <summary>Adds a policy that requires the given scope.</summary>
    public static AuthorizationOptions AddFcgScopePolicy(this AuthorizationOptions options, string scope)
    {
        options.AddPolicy(RequireScopePolicyName(scope), policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(ctx => ctx.User.HasScope(scope)));
        return options;
    }
}
