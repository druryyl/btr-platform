using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace btrade.webapi.Configurations;

public static class PresentationService
{

    public static IServiceCollection AddPresentation(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version??string.Empty;
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BTrade API",
                Version = $"v{version}",
                Description = "BTrade-Api",
                Contact = new OpenApiContact
                {
                    Name = "Elsasa",
                    Email = "drury_yl@yahoo.com",
                    Url = new Uri("https://microsoft.com"),
                },
            });
        });
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidIssuer = configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty))
            };
        });

        services.AddCors(p => p.AddPolicy("corsapp", policyBuilder =>
        {
            policyBuilder.WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin();
        }));

        services.AddHttpContextAccessor();
        
        return services;
    }
}