using BimshireStore.Services.AuthAPI.Data;
using BimshireStore.Services.AuthAPI.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

// Identity
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>() // IMPORTANT THIS MUST APPEAR FIRST RIGHT HERE
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints()
    .AddDefaultTokenProviders();

// Authentication - Bearer Token
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);

// Authentication - Cookies
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultScheme = IdentityConstants.ApplicationScheme;
//     options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
// }).AddIdentityCookies();

// Authorization
builder.Services.AddAuthorizationBuilder();

// Other Services
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

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

app.MapIdentityApi<ApplicationUser>();

app.Run();

async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initilize();
    }
}
