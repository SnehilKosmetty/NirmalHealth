using Microsoft.EntityFrameworkCore;
using NirmalHealth.Core.Entities;

namespace NirmalHealth.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<HospitalSpecialty> HospitalSpecialties => Set<HospitalSpecialty>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSpecialty> DoctorSpecialties => Set<DoctorSpecialty>();
    public DbSet<SlotTemplate> SlotTemplates => Set<SlotTemplate>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<SymptomAnalysis> SymptomAnalyses => Set<SymptomAnalysis>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserRole>(e =>
        {
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        builder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Phone);
            e.HasOne(u => u.Hospital).WithMany(h => h.HospitalAdmins).HasForeignKey(u => u.HospitalId);
        });

        builder.Entity<HospitalSpecialty>(e =>
        {
            e.HasKey(hs => new { hs.HospitalId, hs.SpecialtyId });
            e.HasOne(hs => hs.Hospital).WithMany(h => h.HospitalSpecialties).HasForeignKey(hs => hs.HospitalId);
            e.HasOne(hs => hs.Specialty).WithMany(s => s.HospitalSpecialties).HasForeignKey(hs => hs.SpecialtyId);
        });

        builder.Entity<DoctorSpecialty>(e =>
        {
            e.HasKey(ds => new { ds.DoctorId, ds.SpecialtyId });
            e.HasOne(ds => ds.Doctor).WithMany(d => d.DoctorSpecialties).HasForeignKey(ds => ds.DoctorId);
            e.HasOne(ds => ds.Specialty).WithMany(s => s.DoctorSpecialties).HasForeignKey(ds => ds.SpecialtyId);
        });

        builder.Entity<Doctor>(e =>
        {
            e.HasOne(d => d.Hospital).WithMany(h => h.Doctors).HasForeignKey(d => d.HospitalId);
        });

        builder.Entity<SlotTemplate>(e =>
        {
            e.HasOne(s => s.Doctor).WithMany(d => d.SlotTemplates).HasForeignKey(s => s.DoctorId);
        });

        builder.Entity<AppointmentSlot>(e =>
        {
            e.HasOne(s => s.Doctor).WithMany().HasForeignKey(s => s.DoctorId);
            e.HasIndex(s => new { s.DoctorId, s.Date, s.StartTime }).IsUnique();
        });

        builder.Entity<Appointment>(e =>
        {
            e.HasOne(a => a.Patient).WithMany(u => u.AppointmentsAsPatient).HasForeignKey(a => a.PatientId);
            e.HasOne(a => a.Slot).WithOne(s => s.Appointment!).HasForeignKey<Appointment>(a => a.SlotId);
        });

        builder.Entity<SymptomAnalysis>(e =>
        {
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId);
        });
    }
}
