namespace Fcg.Users.Api.Observability;

/// <summary>Metric names (snake_case) for Users API. Compatible with OpenTelemetry and CloudWatch.</summary>
public static class FcgMetricNames
{
    public const string HttpServerRequestCount = "http.server.request.count";
    public const string HttpServerRequestDuration = "http.server.request.duration";
    public const string HttpServerActiveRequests = "http.server.active_requests";
    public const string UsersCreated = "users.created";
    public const string UsersDeleted = "users.deleted";
    public const string ExceptionsCount = "exceptions.count";

    public const string TagHttpMethod = "http.request.method";
    public const string TagHttpRoute = "http.route";
    public const string TagHttpStatusCode = "http.response.status_code";
    public const string TagExceptionType = "exception.type";
}
