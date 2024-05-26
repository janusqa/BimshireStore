API
===
- dotnet new webapi --use-controllers -o [<project-name>]
- dotnet add Services/BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore.Sqlite
- dotnet add Services/BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore 
- dotnet add Services/BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore.Tools
- dotnet add Services/BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.Authentication.JwtBearer
- dotnet add Services/BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
- dotnet add Services/BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore


Migrations
===
- dotnet ef migrations add AddCouponToCouponApiDb --project [<project-with-dbcontext>] --startup-project [<main-project>]
- dotnet ef database update --startup-project [<main-project>]/[<main-project>].csproj