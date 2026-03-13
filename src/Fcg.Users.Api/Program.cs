using Fcg.Shared.Auth;
using Fcg.Shared.Observability;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Infrastructure.Extensions;
using Fcg.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// DbContext + Application/Infrastructure services
builder.Services.AddUsersInfrastructure(builder.Configuration, builder.Environment);

// JWT Authentication & Authorization (shared FCG)
builder.Services.AddFcgJwtBearer(builder.Configuration);
builder.Services.AddFcgAuthorization();

// Observability
builder.Services.AddProjectObservability(builder.Configuration, "Fcg.Users.Api");

// Controllers
builder.Services.AddControllers();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UsersDbContext>("db", tags: new[] { "ready" });

// OpenAPI + Scalar (desabilitado em Production; use EnableOpenApi: true para forçar)
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
    options.AddDocumentTransformer<Fcg.Users.Api.OpenApi.BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Observability (correlation id, HTTP metrics, exception log + metric)
app.UseFcgObservability();
// Global exception handling
app.UseMiddleware<Fcg.Users.Api.Middleware.ExceptionHandlingMiddleware>();

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

app.MapGet("/api/discovery", (HttpContext ctx) => new
{
    service = "Fcg.Users.Api",
    basePath = ctx.Request.PathBase.Value?.TrimEnd('/') ?? "",
    openApiUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/openapi/v1.json",
    docsUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/scalar/v1",
    healthUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/health"
}).AllowAnonymous();

// Migrations + bootstrap first admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    if (!config.GetValue<bool>("UseInMemoryDatabase"))
        await db.Database.MigrateAsync();

    var bootstrap = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("Bootstrap");
    if (bootstrap.GetValue<bool>("CreateAdminIfNone"))
    {
        var repo = scope.ServiceProvider.GetRequiredService<Fcg.Users.Domain.Repositories.IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<Fcg.Users.Application.Services.IPasswordHasher>();
        var adminCount = await repo.CountAdminsAsync();
        if (adminCount == 0)
        {
            var email = bootstrap["AdminEmail"] ?? "admin@fcg.local";
            var password = bootstrap["AdminPassword"] ?? "ChangeMe@123";
            var name = bootstrap["AdminName"] ?? "System Admin";
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = hasher.Hash(password),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await repo.AddAsync(admin);
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Bootstrap: first admin created with email {Email}", email);
        }
    }
}

app.Run();
