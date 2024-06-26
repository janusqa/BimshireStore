using BimshireStore.Services;
using BimshireStore.Services.IService;
using BimshireStore.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// UI Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    // options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    // we needed to set LoginPath as it was going to the razor page for
    // identity which we are not using yet. Our login page is "Auth/Login"
    // "not /Identity/Login" OR "/Account/Login" 
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Login";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();

// App & API URIs
SD.AppBaseAddress = builder.Configuration["AppUris:BaseUrl"]
    ?? throw new InvalidOperationException("Invalid App base Address");
SD.CouponApiBaseAddress = builder.Configuration["ServiceUris:CouponApi"]
    ?? throw new InvalidOperationException("Invalid CouponAPI base Address");
SD.AuthApiBaseAddress = builder.Configuration["ServiceUris:AuthApi"]
    ?? throw new InvalidOperationException("Invalid AuthAPI base Address");
SD.ProductApiBaseAddress = builder.Configuration["ServiceUris:ProductApi"]
    ?? throw new InvalidOperationException("Invalid ProductAPI base Address");
SD.ShoppingCartApiBaseAddress = builder.Configuration["ServiceUris:ShoppingCartApi"]
    ?? throw new InvalidOperationException("Invalid ShoppingCartAPI base Address");
SD.OrderApiBaseAddress = builder.Configuration["ServiceUris:OrderApi"]
    ?? throw new InvalidOperationException("Invalid OrderAPI base Address");

// HTTPClient
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("BimshireStore")
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
