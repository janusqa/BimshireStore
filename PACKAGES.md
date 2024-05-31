API
===
- dotnet new webapi --use-controllers -o [<project-name>]
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore.Sqlite
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore 
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore.Tools
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.Authentication.JwtBearer
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore

Auth API (additional packages)
===
- dotnet add BimshireStore.Services.AuthAPI package Microsoft.AspNetCore.Identity.EntityFrameworkCore
- dotnet add BimshireStore.Services.AuthAPI package RabbitMQ.Client

UI (additional packages)
===
- dotnet add BimshireStore package Microsoft.IdentityModel.JsonWebTokens

Auth API (additional config)
- add "app.UseAuthentication()" above "app.UseAuthorization()" in program.cs;
- add Identity to services
    ```
    // AuthN
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    ```


Frontend
===
dotnet new mvc -o [<project-name>]


Migrations
===
- dotnet ef migrations add AddCouponToCouponApiDb --project [<project-with-dbcontext>] --startup-project [<main-project>]
- dotnet ef database update --startup-project [<main-project>]/[<main-project>].csproj