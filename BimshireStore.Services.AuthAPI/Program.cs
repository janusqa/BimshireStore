using System.Text;
using AppLib.ServiceBus.Services;
using AppLib.ServiceBus.Services.IService;
using BimshireStore.Services.AuthAPI.Data;
using BimshireStore.Services.AuthAPI.Models;
using BimshireStore.Services.AuthAPI.Services;
using BimshireStore.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
builder.Services.AddScoped<IServiceBusProducer>(x =>
    new ServiceBusProducer(
        builder.Configuration.GetValue<string>("MessageBus:host") ?? throw new InvalidOperationException("Invalid MessageBus Host"),
        builder.Configuration.GetValue<string>("MessageBus:uid") ?? throw new InvalidOperationException("Invalid MessageBus UID"),
        builder.Configuration.GetValue<string>("MessageBus:pid") ?? throw new InvalidOperationException("Invalid MessageBus PID"),
        builder.Configuration.GetValue<string>("MessageBus:TopicAndQueueNames:ExchangeDeadLetter") ?? throw new InvalidOperationException("Invalid MessageBus Dead-letter Exchange"),
        builder.Configuration.GetValue<string>("MessageBus:TopicAndQueueNames:QueueDeadLetter") ?? throw new InvalidOperationException("Invalid MessageBus Dead-letter Queue")
    )
);

// Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthAPI", Version = "v1", });
    x.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference= new OpenApiReference {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

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
