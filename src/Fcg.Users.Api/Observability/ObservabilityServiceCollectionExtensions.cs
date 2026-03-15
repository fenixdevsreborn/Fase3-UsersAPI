using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Fcg.Users.Api.Observability;

/// <summary>Registers ActivitySource, FcgMeters, observability options and OpenTelemetry. Then use app.UseFcgObservability() for middlewares.</summary>
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
        ApplyOtelEnvironmentVariables(options);
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ActivitySource>(FcgActivitySource.Instance);
        services.AddSingleton<FcgMeters>(sp =>
        {
            var opts = sp.GetRequiredService<ObservabilityOptions>();
            return new FcgMeters(opts.ProjectName);
        });
        services.AddSingleton<IObservabilityContextAccessor, ObservabilityContextAccessor>();

        return services;
    }

    /// <summary>Adds OpenTelemetry with ASP.NET Core, HttpClient, Runtime, Process, OTLP export, and service ActivitySource/Meter.</summary>
    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);
        ApplyOtelEnvironmentVariables(options);

        var useOtlp = options.OtlpExportEnabled && !string.IsNullOrWhiteSpace(options.OtlpEndpoint);
        if (!useOtlp)
        {
            return services;
        }

        var endpointUri = new Uri(options.OtlpEndpoint!.TrimEnd('/'));
        var isGrpc = string.Equals(options.OtlpProtocol, "grpc", StringComparison.OrdinalIgnoreCase);

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(asp =>
                {
                    asp.Filter = context => !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
                    asp.RecordException = true;
                });
                tracing.AddHttpClientInstrumentation(http => http.RecordException = true);
                tracing.AddSource(FcgActivitySource.Name);
                if (isGrpc)
                    tracing.AddOtlpExporter(otlp => { otlp.Endpoint = endpointUri; });
                else
                    tracing.AddOtlpExporter(otlp => { otlp.Endpoint = new Uri(endpointUri, "v1/traces"); otlp.Protocol = OtlpExportProtocol.HttpProtobuf; });
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddRuntimeInstrumentation();
                metrics.AddProcessInstrumentation();
                metrics.AddMeter(options.ProjectName);
                if (isGrpc)
                    metrics.AddOtlpExporter(otlp => { otlp.Endpoint = endpointUri; });
                else
                    metrics.AddOtlpExporter(otlp => { otlp.Endpoint = new Uri(endpointUri, "v1/metrics"); otlp.Protocol = OtlpExportProtocol.HttpProtobuf; });
            });

        return services;
    }

    private static void ApplyOtelEnvironmentVariables(ObservabilityOptions options)
    {
        var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
        if (!string.IsNullOrWhiteSpace(serviceName))
            options.ProjectName = serviceName.Trim();

        var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(endpoint))
            options.OtlpEndpoint = endpoint.Trim();

        var protocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL");
        if (!string.IsNullOrWhiteSpace(protocol))
            options.OtlpProtocol = protocol.Trim().ToLowerInvariant();
    }
}
