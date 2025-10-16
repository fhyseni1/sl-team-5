using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MedicationService.Infrastructure.Data
{
    public class MedicationDbContext : DbContext
    {
        public MedicationDbContext(DbContextOptions<MedicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<Medication> Medications { get; set; }
        public DbSet<DrugInteraction> DrugInteractions { get; set; }
        public DbSet<MedicationDose> MedicationDoses { get; set; }
        public DbSet<MedicationReminder> MedicationReminders { get; set; }
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Medication entity
            modelBuilder.Entity<Medication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.GenericName).HasMaxLength(100);
                entity.Property(e => e.Manufacturer).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Instructions).HasMaxLength(500);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.QRCode).HasMaxLength(50);
                entity.Property(e => e.NDCCode).HasMaxLength(50);
                entity.Property(e => e.ScannedImageUrl).HasMaxLength(200);

                // Enum conversions for PostgreSQL
                entity.Property(e => e.Type)
                    .HasConversion(new EnumToStringConverter<MedicationType>())
                    .HasColumnType("varchar");
                entity.Property(e => e.DosageUnit)
                    .HasConversion(new EnumToStringConverter<DosageUnit>())
                    .HasColumnType("varchar");
                entity.Property(e => e.Status)
                    .HasConversion(new EnumToStringConverter<MedicationStatus>())
                    .HasColumnType("varchar");
                entity.Property(e => e.ScanningMethod)
                    .HasConversion(new EnumToStringConverter<ScanningMethod>())
                    .HasColumnType("varchar");

                // Relationships
                entity.HasMany(e => e.Schedules)
                    .WithOne(e => e.Medication)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Doses)
                    .WithOne(e => e.Medication)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.DrugInteractions)
                    .WithOne(e => e.Medication)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Prescription)
                    .WithOne()
                    .HasForeignKey<Prescription>(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DrugInteraction entity
            modelBuilder.Entity<DrugInteraction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.InteractingDrugName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.InteractionDescription).HasMaxLength(500);
                entity.Property(e => e.ClinicalEffect).HasMaxLength(500);
                entity.Property(e => e.ManagementRecommendation).HasMaxLength(500);

                // Enum conversion
                entity.Property(e => e.Severity)
                    .HasConversion(new EnumToStringConverter<InteractionSeverity>())
                    .HasColumnType("varchar");
            });

            // Configure MedicationDose entity
            modelBuilder.Entity<MedicationDose>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Configure MedicationReminder entity
            modelBuilder.Entity<MedicationReminder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.NotificationChannel).HasMaxLength(50);

                // Enum conversion
                entity.Property(e => e.Status)
                    .HasConversion(new EnumToStringConverter<ReminderStatus>())
                    .HasColumnType("varchar");

                // Relationship with MedicationSchedule
                entity.HasOne(e => e.Schedule)
                    .WithMany(e => e.Reminders)
                    .HasForeignKey(e => e.ScheduleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure MedicationSchedule entity
            modelBuilder.Entity<MedicationSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.DaysOfWeek).HasMaxLength(50);

                // Enum conversion
                entity.Property(e => e.Frequency)
                    .HasConversion(new EnumToStringConverter<FrequencyType>())
                    .HasColumnType("varchar");
            });

            // Configure Prescription entity
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PrescriptionNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PrescriberName).HasMaxLength(100);
                entity.Property(e => e.PrescriberContact).HasMaxLength(100);
                entity.Property(e => e.PharmacyName).HasMaxLength(100);
                entity.Property(e => e.PharmacyContact).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Enum conversion
                entity.Property(e => e.Status)
                    .HasConversion(new EnumToStringConverter<PrescriptionStatus>())
                    .HasColumnType("varchar");
            });
        }
    }
}