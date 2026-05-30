using Local_Service_marketPlace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<ProviderProfile> ProviderProfiles { get; set; }
        public DbSet<ProviderCategory> ProviderCategories { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceRequestImage> ServiceRequestImages { get; set; }
        public DbSet<ServiceOffer> ServiceOffers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Complaint> Complaints { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ServiceOffer
            modelBuilder.Entity<ServiceOffer>()
                .HasOne(s => s.ServiceRequest)
                .WithMany(r => r.Offers)
                .HasForeignKey(s => s.ServiceRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceOffer>()
                .HasOne(s => s.ProviderProfile)
                .WithMany(p => p.Offers)
                .HasForeignKey(s => s.ProviderProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ServiceRequest)
                .WithOne(r => r.Booking)
                .HasForeignKey<Booking>(b => b.ServiceRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ServiceOffer)
                .WithOne(o => o.Booking)
                .HasForeignKey<Booking>(b => b.ServiceOfferId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ProviderProfile)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ProviderProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.ProviderProfile)
                .WithMany(p => p.ReviewsReceived)
                .HasForeignKey(r => r.ProviderProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Complaint
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.ReportedByUser)
                .WithMany()
                .HasForeignKey(c => c.ReportedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
