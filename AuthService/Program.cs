using System.Text;
using AuthService.Configuration;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Repositories;
using AuthService.Services;
using AuthService.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// FluentValidation integration (new recommended pattern)
// Requires package: FluentValidation.DependencyInjectionExtensions
builder.Services.AddValidatorsFromAssemblyContaining<AuthRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Interceptor & DbContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<DataContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();

    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .AddInterceptors(auditInterceptor);
});

// JWT configuration via IOptions pattern
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Application / infrastructure services
builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Password hasher from ASP.NET Core Identity
builder.Services.AddScoped<IPasswordHasher<AuthUser>, PasswordHasher<AuthUser>>();

// JWT Authentication setup
var jwtSettingsSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>()
                 ?? throw new InvalidOperationException("Jwt configuration section is missing.");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
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

builder.Services.AddAuthorization(options =>
{
    // Example of a role-based policy that can be used with [Authorize(Policy = "AdminOnly")]
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication / Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
