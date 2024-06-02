using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using BimshireStore.Services.OrderAPI.Models;

namespace BimshireStore.Services.OrderAPI.Data
{
    // To configure Identity we changed DBContext to IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetailss { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // we need this line when using identity or will get error
            // ERROR: InvalidOperationException: The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
            base.OnModelCreating(modelBuilder);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .EnableDetailedErrors()  // detailed error messages
                .EnableSensitiveDataLogging(); // Disable in production
            // .LogTo(Console.WriteLine);
        }
    }
}

