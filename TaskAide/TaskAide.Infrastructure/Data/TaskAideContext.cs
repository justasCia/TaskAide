using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Infrastructure.Data
{
    public class TaskAideContext : IdentityDbContext<User>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Service> Services { get; set; } = default!;
        public DbSet<Provider> Providers { get; set; } = default!;
        public DbSet<ProviderService> ProviderServices { get; set; } = default!;
        public DbSet<Booking> Bookings { get; set; } = default!;
        public DbSet<BookingService> BookingServices { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;

        public TaskAideContext(DbContextOptions<TaskAideContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Provider)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Review)
                .WithOne(r => r.Booking!)
                .HasForeignKey<Booking>(b => b.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
