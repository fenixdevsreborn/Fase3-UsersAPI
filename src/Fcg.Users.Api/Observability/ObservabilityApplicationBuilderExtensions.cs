using Microsoft.AspNetCore.Builder;

namespace Fcg.Users.Api.Observability;

/// <summary>Adds FCG middlewares in order: CorrelationId, HttpMetrics, ExceptionObservability. Call after UseRouting.</summary>
public static class ObservabilityApplicationBuilderExtensions
{
    public static IApplicationBuilder UseFcgObservability(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService(typeof(ObservabilityOptions)) as ObservabilityOptions ?? new ObservabilityOptions();
        if (options.UseCorrelationId)
            app.UseMiddleware<CorrelationIdMiddleware>();
        if (options.UseHttpMetrics)
            app.UseMiddleware<HttpMetricsMiddleware>();
        if (options.UseExceptionObservability)
            app.UseMiddleware<ExceptionObservabilityMiddleware>();
        return app;
    }
}
