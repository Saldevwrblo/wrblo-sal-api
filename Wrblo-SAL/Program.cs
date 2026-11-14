using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Wrblo.SAL.Core.Context;

var builder = WebApplication.CreateBuilder(args);
var platformPort = Environment.GetEnvironmentVariable("PORT");
var isContainerized = string.Equals(
    Environment.GetEnvironmentVariable("CONTAINERIZED"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!string.IsNullOrWhiteSpace(platformPort))
{
    builder.WebHost.UseUrls($"http://+:{platformPort}");
}

var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, opt => opt.CommandTimeout(60)));
builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
    //.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/dotnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!isContainerized)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
