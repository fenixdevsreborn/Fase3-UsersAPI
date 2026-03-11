using Amazon.SQS;
using ms_users.Repositories;
using ms_users.Services;
using ms_users.Messaging;
using Amazon.DynamoDBv2;

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
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllers();
      endpoints.MapGet("/", async context =>
      {
        await context.Response.WriteAsync("Users API running on AWS Lambda");
      });
    });
  }
}