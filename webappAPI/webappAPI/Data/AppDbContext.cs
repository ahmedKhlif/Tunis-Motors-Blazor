using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webappAPI.Models;

namespace webappAPI.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CarListing> CarListings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public DbSet<CarRental> CarRentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CarListing approval workflow
            modelBuilder.Entity<CarListing>()
                .Property(c => c.IsApproved)
                .HasDefaultValue(false);

            // Configure decimal precision for Price
            modelBuilder.Entity<CarListing>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            // Configure decimal precision for EngineSize
            modelBuilder.Entity<CarListing>()
                .Property(c => c.EngineSize)
                .HasColumnType("decimal(4,2)");

            // Configure decimal precision for OrderItem Price
            modelBuilder.Entity<OrderItem>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            // Configure decimal precision for Order TotalAmount
            modelBuilder.Entity<Order>()
                .Property(c => c.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Configure Message relationships to avoid cascade cycles
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure cascade delete for related entities when CarListing is deleted
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.CarListing)
                .WithMany(c => c.Wishlists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.CarListing)
                .WithMany(c => c.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Listing)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ListingId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Category relationships
            modelBuilder.Entity<CarListing>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.CarListings)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CarRental relationships
            modelBuilder.Entity<CarRental>()
                .HasOne(r => r.Car)
                .WithMany()
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarRental>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for CarRental
            modelBuilder.Entity<CarRental>()
                .Property(r => r.DailyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CarRental>()
                .Property(r => r.LateFees)
                .HasColumnType("decimal(18,2)");

            // Seed default roles
            var adminRoleId = "1";
            var managerRoleId = "2";
            var sellerRoleId = "3";
            var buyerRoleId = "4";

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = managerRoleId, Name = "Manager", NormalizedName = "MANAGER" },
                new IdentityRole { Id = sellerRoleId, Name = "Seller", NormalizedName = "SELLER" },
                new IdentityRole { Id = buyerRoleId, Name = "Buyer", NormalizedName = "BUYER" }
            );
        }
    }
}
