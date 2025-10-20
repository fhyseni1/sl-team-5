using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Infrastructure.Data
{
    public class UserHealthDbContext : DbContext
    {
        public UserHealthDbContext(DbContextOptions<UserHealthDbContext> options)
            : base(options)
        {
        }

        // DbSets për çdo entitet
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<HealthMetric> HealthMetrics { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserRelationship> UserRelationships { get; set; }
        public DbSet<SymptomLog> SymptomLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ USER
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);

                entity.Property(e => e.Type)
                    .HasConversion(new EnumToStringConverter<UserType>())
                    .HasColumnType("varchar(50)");

                entity.HasOne(e => e.Profile)
                    .WithOne(e => e.User)
                    .HasForeignKey<UserProfile>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Allergies)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Appointments)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.HealthMetrics)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Notifications)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Relationships)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ USER PROFILE
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.BloodType).HasMaxLength(10);
                entity.Property(e => e.MedicalHistory).HasMaxLength(1000);
                entity.Property(e => e.CurrentConditions).HasMaxLength(1000);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);
                entity.Property(e => e.InsuranceProvider).HasMaxLength(100);
                entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(50);
                entity.Property(e => e.PrimaryCarePhysician).HasMaxLength(100);
                entity.Property(e => e.PrimaryCarePhysicianPhone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
            });

            // ✅ ALLERGY
            modelBuilder.Entity<Allergy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.AllergenName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Symptoms).HasMaxLength(500);
                entity.Property(e => e.Treatment).HasMaxLength(500);
                entity.Property(e => e.DiagnosedBy).HasMaxLength(100);

                entity.Property(e => e.Severity)
                    .HasConversion(new EnumToStringConverter<AllergySeverity>())
                    .HasColumnType("varchar(50)");
            });

            // ✅ APPOINTMENT
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.DoctorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Specialty).HasMaxLength(100);
                entity.Property(e => e.ClinicName).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.Status)
                    .HasConversion(new EnumToStringConverter<AppointmentStatus>())
                    .HasColumnType("varchar(50)");
            });

            // ✅ HEALTH METRIC
            modelBuilder.Entity<HealthMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Device).HasMaxLength(100);

                entity.Property(e => e.Type)
                    .HasConversion(new EnumToStringConverter<HealthMetricType>())
                    .HasColumnType("varchar(50)");
            });

            // ✅ NOTIFICATION
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ActionUrl).HasMaxLength(200);
                entity.Property(e => e.Priority).HasMaxLength(20);

                entity.Property(e => e.Type)
                    .HasConversion(new EnumToStringConverter<NotificationType>())
                    .HasColumnType("varchar(50)");
            });

            // ✅ USER RELATIONSHIP
            modelBuilder.Entity<UserRelationship>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.RelationshipType)
                    .HasConversion(new EnumToStringConverter<RelationshipType>())
                    .HasColumnType("varchar(50)");

                entity.HasOne(e => e.RelatedUser)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ✅ REFRESH TOKEN
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Token);
                entity.Property(e => e.Token).HasMaxLength(500);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
