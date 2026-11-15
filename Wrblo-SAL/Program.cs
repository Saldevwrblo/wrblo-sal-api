using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Wrblo.SAL.Core.Context;
using Wrblo.SAL.Core.Handler;
using Wrblo.SAL.Shared.Entities;
using Wrblo.SAL.Shared.ViewModels;
using Wrblo.SAL.Core.Implementation.Repositories;
using Wrblo.SAL.Core.Implementation.Services;

var builder = WebApplication.CreateBuilder(args);
var platformPort = Environment.GetEnvironmentVariable("PORT");
var isContainerized = string.Equals(
    Environment.GetEnvironmentVariable("CONTAINERIZED"),
    "true",
    StringComparison.OrdinalIgnoreCase);
var enableApiReference = builder.Configuration.GetValue<bool?>("ApiReference:Enabled")
    ?? builder.Environment.IsDevelopment();
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

if (!string.IsNullOrWhiteSpace(platformPort))
{
    builder.WebHost.UseUrls($"http://+:{platformPort}");
}

if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be configured with at least 32 bytes.");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, opt => opt.CommandTimeout(60)));
builder.Services.AddHealthChecks();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => false);
            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Google"));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(Roles.Admin.ToString(), Roles.SuperAdmin.ToString()));
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole(
            Roles.User.ToString(),
            Roles.Admin.ToString(),
            Roles.SuperAdmin.ToString()));
});

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = string.Join(
            "; ",
            context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "The request is invalid."
                    : error.ErrorMessage));

        return new BadRequestObjectResult(ApiResponse<object>.BadRequest(message));
    };
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Wrblo SAL API";
        document.Info.Version = "v1";
        document.Info.Description = "API documentation for Wrblo SAL";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (enableApiReference)
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!isContainerized)
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
