using HomeMarket.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HomeMarket.Api.Data
{
    public class MarketContext : DbContext
    {
        public MarketContext(DbContextOptions<MarketContext> options)
            : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Message> Messages => Set<Message>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var asUtc = new ValueConverter<DateTime, DateTime>(
                toDb => toDb,
                fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));

            modelBuilder.Entity<Account>(account =>
            {
                account.Property(a => a.UserName).HasMaxLength(30);
                account.HasIndex(a => a.UserName).IsUnique();
                account.Property(a => a.CreatedAt).HasConversion(asUtc);
            });

            modelBuilder.Entity<Product>(product =>
            {
                product.Property(p => p.Title).HasMaxLength(80);
                product.Property(p => p.Brand).HasMaxLength(60);
                product.Property(p => p.Maker).HasMaxLength(60);
                product.Property(p => p.Description).HasMaxLength(2000);
                product.Property(p => p.Category).HasMaxLength(40);
                product.HasIndex(p => p.Category);
                product.Property(p => p.Photo).HasMaxLength(200);
                product.Property(p => p.Price).HasPrecision(10, 2);
                product.Property(p => p.CreatedAt).HasConversion(asUtc);
                product.Property(p => p.UpdatedAt).HasConversion(asUtc);
                product.HasOne(p => p.Seller).WithMany(a => a.Listings).HasForeignKey(p => p.SellerId).OnDelete(DeleteBehavior.Cascade);
                product.HasMany(p => p.LikedBy).WithMany(a => a.Liked).UsingEntity(join => join.ToTable("Likes"));
            });

            modelBuilder.Entity<Message>(message =>
            {
                message.Property(m => m.Subject).HasMaxLength(120);
                message.Property(m => m.Body).HasMaxLength(4000);
                message.Property(m => m.SentAt).HasConversion(asUtc);
                message.Property(m => m.ReadAt).HasConversion(new ValueConverter<DateTime?, DateTime?>(
                    toDb => toDb,
                    fromDb => fromDb == null ? null : DateTime.SpecifyKind(fromDb.Value, DateTimeKind.Utc)));
                message.HasIndex(m => new { m.RecipientId, m.SentAt });
                message.HasIndex(m => new { m.SenderId, m.SentAt });
                message.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Cascade);
                message.HasOne(m => m.Recipient).WithMany().HasForeignKey(m => m.RecipientId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
