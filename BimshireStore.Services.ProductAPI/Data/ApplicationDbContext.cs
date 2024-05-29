using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using BimshireStore.Services.ProductAPI.Models;

namespace BimshireStore.Services.ProductAPI.Data
{
    // To configure Identity we changed DBContext to IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // we need this line when using identity or will get error
            // ERROR: InvalidOperationException: The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Samosa",
                    Price = 15,
                    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
                    ImageUrl = "https://placehold.co/603x403",
                    CategoryName = "Appetizer"
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Paneer Tikka",
                    Price = 13.99,
                    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
                    ImageUrl = "https://placehold.co/602x402",
                    CategoryName = "Appetizer"
                },
                new Product
                {
                    ProductId = 3,
                    Name = "Sweet Pie",
                    Price = 10.99,
                    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
                    ImageUrl = "https://placehold.co/601x401",
                    CategoryName = "Dessert"
                },
                new Product
                {
                    ProductId = 4,
                    Name = "Pav Bhaji",
                    Price = 15,
                    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
                    ImageUrl = "https://placehold.co/600x400",
                    CategoryName = "Entree"
                }
            );
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

