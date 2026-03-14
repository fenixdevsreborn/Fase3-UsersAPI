using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Users.Api.Observability;

/// <summary>Registers ActivitySource, FcgMeters and observability options. Then use app.UseFcgObservability() for middlewares.</summary>
public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddProjectObservability(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        string? projectName = null,
        Action<ObservabilityOptions>? configure = null)
    {
        var options = new ObservabilityOptions();
        if (!string.IsNullOrWhiteSpace(projectName))
            options.ProjectName = projectName;
        configuration?.GetSection(ObservabilityOptions.SectionName).Bind(options);
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<ObservabilityOptions>();
            return new ActivitySource(opts.ProjectName);
        });
        services.AddSingleton<FcgMeters>(sp =>
        {
            var opts = sp.GetRequiredService<ObservabilityOptions>();
            return new FcgMeters(opts.ProjectName);
        });
        services.AddSingleton<IObservabilityContextAccessor, ObservabilityContextAccessor>();

        return services;
    }
}
