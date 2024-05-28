using System.Text;
using BimshireStore.Services.AuthAPI.Data;
using BimshireStore.Services.AuthAPI.Models;
using BimshireStore.Services.AuthAPI.Services;
using BimshireStore.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Databases
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetSection("APP_DB_CONNECTIONSTRING").Value
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Data Protection keys
// When using docker container store keys in a db (or alternatively in a bind/volume mount)
builder.Services.AddDataProtection()
    .SetApplicationName("BimshireStore.Services.AuthAPI")
    .PersistKeysToDbContext<ApplicationDbContext>();

// Authentication - Bearer Token
var jwtSettings = new JwtSettings();
builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);

var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services
     .AddAuthentication(x =>
     {
         x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
         x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
         x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

     })
     .AddJwtBearer(jwt =>
     {
         jwt.SaveToken = true;
         jwt.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SigningKey ?? throw new InvalidOperationException())),
             ValidateIssuer = true,
             ValidIssuer = jwtSettings.Issuer,
             ValidAudiences = jwtSettings.Audiences,
             RequireExpirationTime = false,
             ValidateLifetime = true
         };
         jwt.Audience = jwtSettings.Audiences?[0];
         jwt.ClaimsIssuer = jwtSettings.Issuer;

     });

// builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);

// Authentication - Cookies
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultScheme = IdentityConstants.ApplicationScheme;
//     options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
// }).AddIdentityCookies();

// Identity
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>() // IMPORTANT THIS MUST APPEAR FIRST RIGHT HERE
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();
// .AddDefaultTokenProviders();
// .AddApiEndpoints();

// Authorization
builder.Services.AddAuthorizationBuilder();

// Other Services
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IdentityService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//seed the db
await SeedDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// app.MapIdentityApi<ApplicationUser>();

app.Run();

async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initilize();
    }
}