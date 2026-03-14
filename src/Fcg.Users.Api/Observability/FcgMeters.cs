using System.Diagnostics.Metrics;

namespace Fcg.Users.Api.Observability;

/// <summary>Meter facade for Users API: HTTP metrics, users.created/deleted, exceptions.count.</summary>
public sealed class FcgMeters
{
    private readonly Meter _meter;
    private readonly Counter<long> _httpRequestCount;
    private readonly Histogram<double> _httpRequestDuration;
    private readonly UpDownCounter<long> _httpActiveRequests;
    private readonly Counter<long> _usersCreated;
    private readonly Counter<long> _usersDeleted;
    private readonly Counter<long> _exceptionsCount;

    public FcgMeters(string meterName)
    {
        _meter = new Meter(meterName, "1.0.0");
        _httpRequestCount = _meter.CreateCounter<long>(FcgMetricNames.HttpServerRequestCount, "requests", "Total HTTP requests");
        _httpRequestDuration = _meter.CreateHistogram<double>(FcgMetricNames.HttpServerRequestDuration, "s", "Request duration in seconds");
        _httpActiveRequests = _meter.CreateUpDownCounter<long>(FcgMetricNames.HttpServerActiveRequests, "requests", "Active HTTP requests");
        _usersCreated = _meter.CreateCounter<long>(FcgMetricNames.UsersCreated);
        _usersDeleted = _meter.CreateCounter<long>(FcgMetricNames.UsersDeleted);
        _exceptionsCount = _meter.CreateCounter<long>(FcgMetricNames.ExceptionsCount);
    }

    public Meter Meter => _meter;

    public void RecordHttpRequest(string method, string route, int statusCode, double durationSeconds)
    {
        _httpRequestCount.Add(1,
            new KeyValuePair<string, object?>(FcgMetricNames.TagHttpMethod, method),
            new KeyValuePair<string, object?>(FcgMetricNames.TagHttpRoute, route),
            new KeyValuePair<string, object?>(FcgMetricNames.TagHttpStatusCode, statusCode));
        _httpRequestDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>(FcgMetricNames.TagHttpMethod, method),
            new KeyValuePair<string, object?>(FcgMetricNames.TagHttpRoute, route),
            new KeyValuePair<string, object?>(FcgMetricNames.TagHttpStatusCode, statusCode));
    }

    public void RecordHttpRequestStart() => _httpActiveRequests.Add(1);
    public void RecordHttpRequestStop() => _httpActiveRequests.Add(-1);
    public void RecordUserCreated() => _usersCreated.Add(1);
    public void RecordUserDeleted() => _usersDeleted.Add(1);

    public void RecordException(string exceptionType, int? httpStatusCode = null)
    {
        var tags = new List<KeyValuePair<string, object?>> { new(FcgMetricNames.TagExceptionType, exceptionType) };
        if (httpStatusCode.HasValue)
            tags.Add(new KeyValuePair<string, object?>(FcgMetricNames.TagHttpStatusCode, httpStatusCode.Value));
        _exceptionsCount.Add(1, tags.ToArray());
    }
}
