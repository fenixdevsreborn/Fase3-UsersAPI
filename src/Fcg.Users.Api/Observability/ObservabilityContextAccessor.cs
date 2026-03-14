namespace Fcg.Users.Api.Observability;

/// <summary>Implementation that reads from current Activity (set by middleware).</summary>
public sealed class ObservabilityContextAccessor : IObservabilityContextAccessor
{
    public string? TraceId => ObservabilityContext.GetCurrentTraceId();
    public string? CorrelationId => ObservabilityContext.GetCurrentCorrelationId();
}
