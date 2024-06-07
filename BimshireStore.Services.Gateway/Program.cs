using System.Text;
using BimshireStore.Services.Gateway.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Authentication - Bearer Token
var jwtSettings = new JwtSettings();
// builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);
builder.Configuration.GetSection(nameof(JwtSettings)).Bind(jwtSettings);

var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services
     .AddAuthentication(x =>
     {
         x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
         x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
         x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

     })
     .AddJwtBearer(x =>
     {
         x.SaveToken = true;
         x.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SigningKey ?? throw new InvalidOperationException())),
             ValidateIssuer = true,
             ValidIssuer = jwtSettings.Issuer,
             ValidAudiences = jwtSettings.Audiences,
             RequireExpirationTime = false,
             ValidateLifetime = true
         };
         x.Audience = jwtSettings.Audiences?[0];
         x.ClaimsIssuer = jwtSettings?.Issuer;
         // Fix broken remapping of claims
         // https://nestenius.se/2023/06/02/debugging-jwtbearer-claim-problems-in-asp-net-core/
         x.MapInboundClaims = false; // Microsoft remaps claims differently. Stop it!!!
         x.TokenValidationParameters.RoleClaimType = "role"; // Microsoft remaps claims differently. Stop it!!!
         x.TokenValidationParameters.NameClaimType = "name"; // Microsoft remaps claims differently. Stop it!!!
     });

// Ocelot Config
if (builder.Environment.EnvironmentName.ToString().ToLower().Equals("production"))
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
}
else // is development env
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
}

// Other Services
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

await app.UseOcelot();

app.MapGet("/", () => "Hello World!");

app.Run();
