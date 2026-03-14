namespace Fcg.Users.Api.Observability;

/// <summary>Access to current observability context (trace/correlation). Use in services that publish events or audit.</summary>
public interface IObservabilityContextAccessor
{
    string? TraceId { get; }
    string? CorrelationId { get; }
}
