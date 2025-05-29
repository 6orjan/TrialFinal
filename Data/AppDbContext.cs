using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Room> Rooms { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Guest entity
            modelBuilder.Entity<Guest>(entity =>
            {
                entity.ToTable("Guests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(400);
                entity.Property(e => e.DOB).IsRequired();
                entity.Property(e => e.Address).IsRequired().HasMaxLength(600);
                entity.Property(e => e.Nationality).IsRequired();
                entity.Property(e => e.CheckInDate).IsRequired();
                entity.Property(e => e.CheckOutDate).IsRequired();
                entity.Property(e => e.RoomId).IsRequired();

                // Configure foreign key relationship
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.Guests)
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
            });

            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Rooms");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
                entity.Property(e => e.Number).IsRequired();
                entity.Property(e => e.Floor).IsRequired();
                entity.Property(e => e.Type).IsRequired();
            });
        }

        // Optional: Add PostgreSQL-specific configurations if needed
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                
                optionsBuilder.UseNpgsql("Host=localhost;Database=HotelDB;Username=postgres;Password=yourpassword");
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            }
        }
    }
}