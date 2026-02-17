using AuthService.Common.Swagger;
using AuthService.Configuration;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Helpers;
using AuthService.Repositories.AuthRefreshTokens;
using AuthService.Repositories.AuthUsers;
using AuthService.Repositories.Roles;
using AuthService.Repositories.Tenants;
using AuthService.Services.Auths;
using AuthService.Services.Passwords;
using AuthService.Services.Roles;
using AuthService.Services.Tenants;
using AuthService.Services.Tokens;
using AuthService.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace AuthService.Extensions;

/// <summary>
/// Centralizes dependency injection registration for the auth service.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHealthChecks().AddDbContextCheck<DataContext>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: Bearer {your JWT}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

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
                    Array.Empty<string>()
                }
            });

            options.OperationFilter<ApiResultOperationFilter>();
        });


        AddDbContext(services, configuration);
        AddJwtConfiguration(services, configuration);
        AddSecurityHeaders(services, configuration);
        AddApplicationServices(services);
        AddJwtAuthentication(services, configuration);
        AddCorsConfiguration(services);

        return services;
    }

    private static void AddSecurityHeaders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SecurityHeadersOptions>(
            configuration.GetSection(SecurityHeadersOptions.SectionName));
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<DataContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            options.UseNpgsql(configuration.GetConnectionString("Default"))
                   .AddInterceptors(auditInterceptor);
        });
    }

    private static void AddJwtConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IAuthService, Services.Auths.AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher<AuthUser>, PasswordHasher<AuthUser>>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthRefreshTokenRepository, AuthRefreshTokenRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantService, TenantService>();
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
        
        var isDevelopment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development" ||
                           Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !isDevelopment;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    }

    private static void AddCorsConfiguration(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy.SetIsOriginAllowed(origin => 
                        new Uri(origin).Host == "localhost" || 
                        new Uri(origin).Host == "127.0.0.1")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
    }
}
