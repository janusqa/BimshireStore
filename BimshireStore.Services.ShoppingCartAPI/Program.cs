using System.Text;
using AppLib.ServiceBus.Services;
using AppLib.ServiceBus.Services.IService;
using BimshireStore.Services.ShoppingCartAPI.Data;
using BimshireStore.Services.ShoppingCartAPI.Models;
using BimshireStore.Services.ShoppingCartAPI.Services;
using BimshireStore.Services.ShoppingCartAPI.Services.IService;
using BimshireStore.Services.ShoppingCartAPI.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
    .SetApplicationName("BimshireStore.Services.CouponAPI")
    .PersistKeysToDbContext<ApplicationDbContext>();

// Authentication 
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(nameof(JwtSettings)).Bind(jwtSettings);

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0
// The Options Pattern (This is for if we wish to inject Jwt Settings in any class)
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

builder.Services.AddAuthentication(x =>
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
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudiences = jwtSettings?.Audiences,
        RequireExpirationTime = false,
        ValidateLifetime = true
    };
    x.Audience = jwtSettings?.Audiences?[0];
    x.ClaimsIssuer = jwtSettings?.Issuer;
    // Fix broken remapping of claims
    // https://nestenius.se/2023/06/02/debugging-jwtbearer-claim-problems-in-asp-net-core/
    x.MapInboundClaims = false; // Microsoft remaps claims differently. Stop it!!!
    x.TokenValidationParameters.RoleClaimType = "role"; // Microsoft remaps claims differently. Stop it!!!
    x.TokenValidationParameters.NameClaimType = "name"; // Microsoft remaps claims differently. Stop it!!!
});

// Authorization
builder.Services.AddAuthorizationBuilder();

// Other Services
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddTransient<ServiceAccount>(); // NOTE REGISTERED AS TRANSIENT
builder.Services.AddScoped<IServiceBusProducer>(x =>
    new ServiceBusProducer(
        builder.Configuration.GetValue<string>("MessageBus:host") ?? throw new InvalidOperationException("Invalid MessageBus Host"),
        builder.Configuration.GetValue<string>("MessageBus:uid") ?? throw new InvalidOperationException("Invalid MessageBus UID"),
        builder.Configuration.GetValue<string>("MessageBus:pid") ?? throw new InvalidOperationException("Invalid MessageBus PID"),
        builder.Configuration.GetValue<string>("MessageBus:TopicAndQueueNames:ExchangeDeadLetter") ?? throw new InvalidOperationException("Invalid MessageBus Dead-letter Exchange"),
        builder.Configuration.GetValue<string>("MessageBus:TopicAndQueueNames:QueueDeadLetter") ?? throw new InvalidOperationException("Invalid MessageBus Dead-letter Queue")
    )
);

// API URIs
SD.CouponApiBaseAddress = builder.Configuration["ServiceUris:CouponApi"]
    ?? throw new InvalidOperationException("Invalid CouponAPI base Address");
SD.ProductApiBaseAddress = builder.Configuration["ServiceUris:ProductApi"]
    ?? throw new InvalidOperationException("Invalid ProductAPI base Address");

// HTTPClient
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("BimshireStore")
.ConfigurePrimaryHttpMessageHandler(() =>
    // !!! DISABLE IN PROD. THIS IS TO BYPASS CHECKING SSL CERT AUTH FOR DEV PURPOSES !!!
    new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }
).AddHttpMessageHandler<ServiceAccount>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "ShoppingCartAPI", Version = "v1", });
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

app.Run();

async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initilize();
    }
}