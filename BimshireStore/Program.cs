using BimshireStore.Service;
using BimshireStore.Services;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// API URIs
SD.CouponApiBaseAddress = builder.Configuration["ServiceUris:CouponApi"]
    ?? throw new InvalidOperationException("Invalid CouponAPI base Address");
SD.AuthApiBaseAddress = builder.Configuration["ServiceUris:AuthApi"]
    ?? throw new InvalidOperationException("Invalid AuthAPI base Address");

// HTTPClient
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("BimshireStoreApi")
.ConfigurePrimaryHttpMessageHandler(() =>
    // !!! DISABLE IN PROD. THIS IS TO BYPASS CHECKING SSL CERT AUTH FOR DEV PURPOSES !!!
    new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
