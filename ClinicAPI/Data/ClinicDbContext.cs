using ClinicAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Data;

public class ClinicDbContext : IdentityDbContext<ApplicationUser>
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<LeavePeriod> LeavePeriods => Set<LeavePeriod>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<VisitRecord> VisitRecords => Set<VisitRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser unique constraints
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.CPRNumber).IsUnique().HasFilter("[CPRNumber] IS NOT NULL");
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.PatientReferenceNumber).IsUnique().HasFilter("[PatientReferenceNumber] IS NOT NULL");

        // Patient 1-to-1 with ApplicationUser
        builder.Entity<Patient>()
            .HasOne(p => p.ApplicationUser)
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.ApplicationUserId);
        builder.Entity<Patient>()
            .HasIndex(p => p.CPRNumber).IsUnique();
        builder.Entity<Patient>()
            .HasIndex(p => p.PatientReferenceNumber).IsUnique();

        // Doctor 1-to-1 with ApplicationUser
        builder.Entity<Doctor>()
            .HasOne(d => d.ApplicationUser)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.ApplicationUserId);
        builder.Entity<Doctor>()
            .HasIndex(d => d.LicenseNumber).IsUnique();

        // DoctorSpecialization composite PK
        builder.Entity<DoctorSpecialization>()
            .HasKey(ds => new { ds.DoctorId, ds.SpecializationId });

        // Appointment FK restrictions to avoid multiple cascade paths
        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Appointment>()
            .HasOne(a => a.Specialization)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        // VisitRecord 1-to-1 with Appointment
        builder.Entity<VisitRecord>()
            .HasOne(vr => vr.Appointment)
            .WithOne(a => a.VisitRecord)
            .HasForeignKey<VisitRecord>(vr => vr.AppointmentId);

        // Notification FK
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Notification>()
            .HasOne(n => n.RelatedAppointment)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.RelatedAppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Store enums as strings for readability
        builder.Entity<ApplicationUser>()
            .Property(u => u.Role).HasConversion<string>();
        builder.Entity<Appointment>()
            .Property(a => a.Status).HasConversion<string>();
        builder.Entity<Notification>()
            .Property(n => n.NotificationType).HasConversion<string>();
    }
}
