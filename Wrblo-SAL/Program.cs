using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Wrblo.SAL.Core.Context;

var builder = WebApplication.CreateBuilder(args);
var platformPort = Environment.GetEnvironmentVariable("PORT");
var isContainerized = string.Equals(
    Environment.GetEnvironmentVariable("CONTAINERIZED"),
    "true",
    StringComparison.OrdinalIgnoreCase);
var enableApiReference = builder.Configuration.GetValue<bool?>("ApiReference:Enabled")
    ?? builder.Environment.IsDevelopment();

if (!string.IsNullOrWhiteSpace(platformPort))
{
    builder.WebHost.UseUrls($"http://+:{platformPort}");
}

var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, opt => opt.CommandTimeout(60)));
builder.Services.AddHealthChecks();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
    //.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
