API
===
- dotnet new webapi --use-controllers -o [<project-name>]
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore.Sqlite
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore 
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.EntityFrameworkCore.Tools
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.Authentication.JwtBearer
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
- dotnet add BimshireStore.Services.CouponAPI package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore

Frontend
===
dotnet new mvc -o [<project-name>]


Migrations
===
- dotnet ef migrations add AddCouponToCouponApiDb --project [<project-with-dbcontext>] --startup-project [<main-project>]
- dotnet ef database update --startup-project [<main-project>]/[<main-project>].csproj