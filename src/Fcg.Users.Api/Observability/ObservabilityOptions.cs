namespace Fcg.Users.Api.Observability;

/// <summary>Options for AddProjectObservability and OpenTelemetry. Supports OTEL_* environment variables.</summary>
public class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>Service name (resource attribute and ActivitySource/Meter name). Override via OTEL_SERVICE_NAME.</summary>
    public string ProjectName { get; set; } = FcgActivitySource.Name;

    public bool UseCorrelationId { get; set; } = true;
    public bool UseHttpMetrics { get; set; } = true;
    public bool UseExceptionObservability { get; set; } = true;

    /// <summary>OTLP endpoint (e.g. http://localhost:4317). Override via OTEL_EXPORTER_OTLP_ENDPOINT.</summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>OTLP protocol: grpc or http/protobuf. Override via OTEL_EXPORTER_OTLP_PROTOCOL.</summary>
    public string OtlpProtocol { get; set; } = "grpc";

    /// <summary>If true, OpenTelemetry and OTLP export are enabled. When false, only local metrics/traces (e.g. console) apply.</summary>
    public bool OtlpExportEnabled { get; set; } = true;
}
