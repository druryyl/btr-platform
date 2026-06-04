using btrade.webapi.Configurations;
using btrade.webapi.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();
app
    .UseSwagger()
    .UseSwaggerUI()
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization()
    .UseCors("corsapp")
    .UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();
app.Run();