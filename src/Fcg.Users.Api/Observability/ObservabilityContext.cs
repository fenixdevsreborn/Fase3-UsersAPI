using System.Diagnostics;

namespace Fcg.Users.Api.Observability;

/// <summary>Reads and sets observability context from Activity (trace id, correlation id).</summary>
public static class ObservabilityContext
{
    public const string CorrelationIdTag = "correlation.id";

    public static string? GetCurrentTraceId() => Activity.Current?.TraceId.ToString();
    public static string? GetCurrentSpanId() => Activity.Current?.SpanId.ToString();
    public static string? GetCurrentCorrelationId() =>
        Activity.Current?.GetTagItem(CorrelationIdTag) as string;

    public static void SetCorrelationId(string correlationId)
    {
        Activity.Current?.SetTag(CorrelationIdTag, correlationId);
    }

    public static string SetCorrelationIdOrDefault(string? incoming, Func<string> generate)
    {
        var value = !string.IsNullOrWhiteSpace(incoming) ? incoming.Trim() : generate();
        SetCorrelationId(value);
        return value;
    }
}
