using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Repositories;
using Fcg.Users.Infrastructure.Authentication;
using Fcg.Users.Infrastructure.Persistence;
using Fcg.Users.Infrastructure.Repositories;
using Fcg.Users.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fcg.Users.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment = null)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase")
            || string.Equals(hostEnvironment?.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
        {
            services.AddDbContext<UsersDbContext>(options =>
                options.UseInMemoryDatabase("FcgUsersTests"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("UsersDb")
                ?? throw new InvalidOperationException("ConnectionStrings:UsersDb is required.");
            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<JwtSigningOptions>(configuration.GetSection(JwtSigningOptions.SectionName));
        services.Configure<BootstrapOptions>(configuration.GetSection(BootstrapOptions.SectionName));

        RegisterRsaKeyProvider(services, configuration);
        services.AddSingleton<IJwksService, JwksService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    private static void RegisterRsaKeyProvider(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Jwt:Signing:Provider"] ?? configuration.GetValue<string>("Jwt:Signing:Provider") ?? "File";
        switch (provider)
        {
            case "File":
                services.AddSingleton<IRsaKeyProvider, FileRsaKeyProvider>();
                break;
            case "Environment":
                services.AddSingleton<IRsaKeyProvider, EnvironmentRsaKeyProvider>();
                break;
            case "AwsParameterStore":
                services.AddSingleton<IRsaKeyProvider, AwsParameterStoreRsaKeyProvider>();
                break;
            case "AwsSecretsManager":
                services.AddSingleton<IRsaKeyProvider, AwsSecretsManagerRsaKeyProvider>();
                break;
            default:
                throw new InvalidOperationException($"Jwt:Signing:Provider '{provider}' is not supported. Use File, Environment, AwsParameterStore, or AwsSecretsManager.");
        }
    }
}

public class BootstrapOptions
{
    public const string SectionName = "Bootstrap";
    public bool CreateAdminIfNone { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@fcg.local";
    public string AdminPassword { get; set; } = "ChangeMe@123";
    public string AdminName { get; set; } = "System Admin";
}
