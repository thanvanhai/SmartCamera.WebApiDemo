using Microsoft.EntityFrameworkCore;
using SmartCamera.WebApiDemo.Models;

namespace SmartCamera.WebApiDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Recording> Recordings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Camera configuration
            modelBuilder.Entity<Camera>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.HasIndex(e => e.IpAddress).IsUnique();
                entity.HasIndex(e => e.Name);
            });

            // Event configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Camera)
                      .WithMany(c => c.Events)
                      .HasForeignKey(e => e.CameraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.CameraId, e.Timestamp });
                entity.HasIndex(e => e.Timestamp);
            });

            // Recording configuration
            modelBuilder.Entity<Recording>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Camera)
                      .WithMany(c => c.Recordings)
                      .HasForeignKey(e => e.CameraId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.CameraId, e.StartTime });
                entity.HasIndex(e => e.StartTime);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@smartcamera.com",
                    //PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    PasswordHash = "$2a$11$WMUFY76/1ERy35BGGMBeduLE.rabzN/wWPvwE0GSOR2M/HXgJ8hLK",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRole.SuperAdmin,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 8, 29)
                }
            );

            // Seed sample camera
            modelBuilder.Entity<Camera>().HasData(
                new Camera
                {
                    Id = 1,
                    Name = "Main Entrance",
                    IpAddress = "192.168.1.100",
                    Port = 554,
                    Username = "admin",
                    Password = "123456",
                    Location = "Main Building - Entrance",
                    Description = "Camera giám sát lối vào chính",
                    Type = CameraType.IP,
                    Status = CameraStatus.Offline,
                    IsActive = true,
                    RecordingEnabled = true,
                    MotionDetectionEnabled = true,
                    FaceRecognitionEnabled = false,
                    CreatedAt = new DateTime(2025, 8, 29)
                }
            );
        }
    }
}