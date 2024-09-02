using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Scenarios.AzureB2C.WebApi.Database;
using Scenarios.AzureB2C.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure authentication with Azure B2C:
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(o =>
    {
        builder.Configuration.Bind("AzureAdB2C", o);
        o.MapInboundClaims = false;
        o.TokenValidationParameters.NameClaimType = "name";

    }, o => builder.Configuration.Bind("AzureAdB2C", o));

// Configure the database:
var connectionString = builder.Configuration.GetConnectionString("ApplicationDb");
builder.Services.AddDbContextFactory<ApplicationDbContext>(o => o.UseSqlServer(connectionString));

// Configure MS Identity:
builder.Services.AddScoped<UserPersistenceMiddleware>();

// Enable CORS:
builder.Services.AddCors();

// Configure Swagger:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(o =>
{
    o.AllowCredentials();
    o.AllowAnyMethod();
    o.AllowAnyHeader();
    o.WithOrigins("https://localhost:7010", "http://localhost:5201");
});

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserPersistenceMiddleware>();

app.MapGet("claims", (ClaimsPrincipal user) => user.Claims.Select(c => new { c.Type, c.Value })).RequireAuthorization();

app.Run();
