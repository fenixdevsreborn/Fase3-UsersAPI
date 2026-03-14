using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fcg.Users.Api.Observability;

/// <summary>Records http.server.request.count, http.server.request.duration and http.server.active_requests.</summary>
public sealed class HttpMetricsMiddleware
{
    private readonly RequestDelegate _next;

    public HttpMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, FcgMeters meters)
    {
        meters.RecordHttpRequestStart();
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            meters.RecordHttpRequestStop();
            var method = context.Request.Method;
            var route = GetRouteTemplateOrDefault(context);
            var statusCode = context.Response.StatusCode;
            meters.RecordHttpRequest(method, route, statusCode, sw.Elapsed.TotalSeconds);
        }
    }

    private static string GetRouteTemplateOrDefault(HttpContext context)
    {
        var endpoint = context.GetEndpoint() as RouteEndpoint;
        var template = endpoint?.RoutePattern?.RawText;
        return string.IsNullOrEmpty(template) ? "unknown" : template;
    }
}
