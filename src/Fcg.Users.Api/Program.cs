using Fcg.Users.Api.Extensions;
using Fcg.Users.Api.Middleware;
using Fcg.Users.Api.Observability;
using Fcg.Users.Api.OpenApi;
using Fcg.Users.Infrastructure.Extensions;
using Fcg.Users.Infrastructure.Persistence;
using Fcg.Users.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddUsersInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddUsersApiAuth(builder.Configuration);
builder.Services.AddUsersApiObservability(builder.Configuration, "Fcg.Users.Api");
builder.Services.AddOpenTelemetryObservability(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<UsersDbContext>("db", tags: new[] { "ready" });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info ??= new Microsoft.OpenApi.OpenApiInfo();
        document.Info.Title = "FCG Users API";
        document.Info.Version = "v1";
        document.Info.Description = "User management and authentication for FCG Cloud Platform.";
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment() && !string.Equals(app.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
{
    var issuer = app.Configuration.GetSection("Jwt:Issuer").Value?.Trim();
    if (string.IsNullOrWhiteSpace(issuer))
        throw new InvalidOperationException("Jwt:Issuer must be set in production (e.g. https://users-api.example.com).");
}

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseFcgObservability();

if (!app.Environment.IsDevelopment() && !string.Equals(app.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

var enableOpenApi = !app.Environment.IsProduction() || app.Configuration.GetValue<bool>("EnableOpenApi");
if (enableOpenApi)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("FCG Users API"));
}

app.MapGet("/.well-known/openid-configuration", (HttpContext ctx, IOptions<Fcg.Users.Contracts.Auth.JwtOptions> jwt) =>
{
    var issuer = !string.IsNullOrWhiteSpace(jwt.Value.Issuer)
        ? jwt.Value.Issuer.TrimEnd('/')
        : $"{ctx.Request.Scheme}://{ctx.Request.Host.Value}{ctx.Request.PathBase.Value?.TrimEnd('/') ?? ""}";
    return Results.Ok(new
    {
        issuer,
        jwks_uri = $"{issuer}/.well-known/jwks.json",
        authorization_endpoint = $"{issuer}/auth/login",
        response_types_supported = new[] { "token" },
        subject_types_supported = new[] { "public" },
        id_token_signing_alg_values_supported = new[] { "RS256" },
        scopes_supported = new[] { "openid", "profile" },
        token_endpoint = (string?)null,
        userinfo_endpoint = (string?)null
    });
}).AllowAnonymous();

app.MapGet("/api/discovery", (HttpContext ctx) => new
{
    service = "Fcg.Users.Api",
    basePath = ctx.Request.PathBase.Value?.TrimEnd('/') ?? "",
    openApiUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/openapi/v1.json",
    docsUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/scalar/v1",
    healthUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/health"
}).AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (!config.GetValue<bool>("UseInMemoryDatabase"))
    {
        var connectionString = config.GetConnectionString("UsersDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
            await PostgresDatabaseEnsurer.EnsureExistsAsync(connectionString);
        await db.Database.MigrateAsync();
    }

    await scope.ServiceProvider.GetRequiredService<ISeederRunner>().RunAsync();
}

app.Run();
