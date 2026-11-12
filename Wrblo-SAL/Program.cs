using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Wrblo.SAL.Core.Context;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, opt => opt.CommandTimeout(60)));
builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
    //.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
