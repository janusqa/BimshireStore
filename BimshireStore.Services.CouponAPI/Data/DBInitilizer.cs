
using Microsoft.EntityFrameworkCore;

namespace BimshireStore.Services.CouponAPI.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        // private readonly IUnitOfWork _uow;
        // private readonly RoleManager<ApplicationRole> _rm;
        // private readonly UserManager<ApplicationUser> _um;        

        public DbInitializer(
            ApplicationDbContext db
        // IUnitOfWork uow,
        // RoleManager<ApplicationRole> rm,
        // UserManager<ApplicationUser> um        
        )
        {
            _db = db;
            // _uow = uow;
            // _rm = rm;
            // _um = um;            
        }

        public async Task Initilize()
        {
            // ****
            // 1. Run any unapplied migrations
            // ****
            try
            {
                if (_db.Database.GetPendingMigrations().Any()) await _db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration Error: {ex.Message}");
            }

            // ****
            // 2. Create Triggers
            // ****
            // var triggers = new List<string>();

            // foreach (var (_, Table) in SD.dbEntity)
            // {
            //     triggers.Add($@"
            //         DROP TRIGGER IF EXISTS {Table}_update_updated_date;
            //         CREATE TRIGGER {Table}_update_updated_date 
            //         AFTER UPDATE ON {Table}
            //         BEGIN
            //             UPDATE {Table} 
            //             SET 
            //                 UpdatedDate = DATETIME('now') 
            //             WHERE id = NEW.id;
            //         END;      
            //     ");
            // }

            // var transaction = _uow.Transaction();
            // // await _uow.Categories.ExecuteSqlAsync(@$"USE {SD.dbName};", []); 
            // foreach (var trigger in triggers)
            // {
            //     await _uow.ApplicationUsers.ExecuteSqlAsync(trigger, []);
            // }
            // transaction.Commit();

            // ****
            // 3. Create Roles if the do not already exist
            // ****
            // var roles = new List<string> {
            //     SD.Role_Customer,
            //     SD.Role_Admin,
            //     SD.Role_SuperAdmin,
            //     SD.Role_Employee,
            // };

            // foreach (var role in roles)
            // {
            //     if (!(await _rm.RoleExistsAsync(role)))
            //     {
            //         await _rm.CreateAsync(new ApplicationRole(role));
            //     }
            // }

            // ****
            // 4. Create Admin user
            // ****
            // if (await _rm.RoleExistsAsync(SD.Role_Admin))
            // {
            //     var adminEmail = "admin@retrievo.net";
            //     var adminUser = await _um.FindByNameAsync(adminEmail);
            //     if (adminUser is null)
            //     {
            //         await _um.CreateAsync(new ApplicationUser
            //         {
            //             UserName = adminEmail,
            //             Email = adminEmail,
            //             EmailConfirmed = true,
            //             UserSecret = BcryptUtils.CreateSalt(),
            //         }, "P@ssw0rd");

            //         adminUser = await _um.FindByNameAsync(adminEmail);
            //         if (adminUser is not null)
            //         {
            //             await _um.AddToRolesAsync(adminUser, [SD.Role_SuperAdmin, SD.Role_Admin, SD.Role_Employee, SD.Role_Customer]);
            //         }
            //     }
            // }

            return;
        }
    }
}