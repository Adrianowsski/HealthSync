using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HealthSync.Shared.Models;

namespace HealthSync.Shared.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    
    public DbSet<NotificationReadStatus> NotificationReadStatuses => Set<NotificationReadStatus>();
    public DbSet<RegistrationCode> RegistrationCodes => Set<RegistrationCode>();
    public DbSet<SiteContent> SiteContents => Set<SiteContent>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Appointment>()
            .HasOne(a => a.DoctorProfile)
            .WithMany()
            .HasForeignKey(a => a.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict); // <--- Zapobiega konfliktowi

        builder.Entity<Appointment>()
            .HasOne(a => a.PatientProfile)
            .WithMany()
            .HasForeignKey(a => a.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict); // <--- Tak samo tutaj
        
        builder.Entity<NotificationReadStatus>()
            .HasIndex(rs => new { rs.NotificationId, rs.UserId })
            .IsUnique();
    }

}