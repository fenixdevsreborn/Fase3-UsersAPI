using Amazon;
using Amazon.DynamoDBv2;
using Amazon.SQS;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.OpenApi.Models;
using ms_users.Messaging;
using ms_users.Observability;
using ms_users.Repositories;
using ms_users.Services;

namespace ms_users;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddControllers();

    services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
    services.AddAWSService<IAmazonSQS>();
    services.AddAWSService<IAmazonDynamoDB>();

    services.AddScoped<UserRepository>();
    services.AddScoped<UserService>();
    services.AddScoped<EventPublisher>();

    services.AddHttpContextAccessor();

    services.AddHealthChecks();

    AWSSDKHandler.RegisterXRayForAllServices();

    // ADD: Swagger/OpenAPI configuration
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Users API",
            Version = "v1",
            Description = "Microservice for user management in Fase 3 ecosystem",
            Contact = new OpenApiContact
            {
                Name = "Fenix Devs",
                Url = new Uri("https://github.com/fenixdevsreborn")
            },
            License = new OpenApiLicense
            {
                Name = "MIT"
            }
        });

        // Add security definition for Cognito/JWT
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
                },
                new string[] { }
            }
            });

        // XML comments for endpoint documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (System.IO.File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });
    }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Users API v1");
        options.RoutePrefix = string.Empty;  // Serve Swagger UI at root
    });

        app.UseRouting();

    app.UseMiddleware<XRayMiddleware>();

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
        endpoints.MapHealthChecks("/ready");
    });
  }
}