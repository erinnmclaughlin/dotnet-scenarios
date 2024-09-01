using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(o =>
    {
        builder.Configuration.Bind("AzureAdB2C", o);
        o.MapInboundClaims = false;
        o.TokenValidationParameters.NameClaimType = "name";

    }, o => builder.Configuration.Bind("AzureAdB2C", o));

builder.Services.AddCors();

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

app.MapGet("claims", (ClaimsPrincipal user) => user.Claims.Select(c => new { c.Type, c.Value })).RequireAuthorization();

app.Run();
