using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NirmalHealth.Core.Entities;
using NirmalHealth.Core.Constants;

namespace NirmalHealth.Infrastructure.Data;

public static class SeedData
{
    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "NirmalHealth_Salt");
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        if (await db.Roles.AnyAsync(ct)) return;

        logger.LogInformation("Seeding database...");

        // SQL Server: IDENTITY_INSERT must be ON on the same connection as the INSERT. Use a transaction so one connection is used.
        var roles = new[]
        {
            new Role { Id = 1, Name = RoleNames.SuperAdmin },
            new Role { Id = 2, Name = RoleNames.HospitalAdmin },
            new Role { Id = 3, Name = RoleNames.Patient }
        };
        await db.Roles.AddRangeAsync(roles);
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Roles] ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Roles] OFF", ct);
            await tx.CommitAsync(ct);
        }
        db.ChangeTracker.Clear();

        var specialties = new List<Specialty>();
        var specialtyNames = new[] { "General Medicine", "Emergency", "Pediatrics", "Cardiology", "Dermatology", "Gynecology", "Orthopedics" };
        for (var i = 0; i < specialtyNames.Length; i++)
            specialties.Add(new Specialty { Id = i + 1, Name = specialtyNames[i] });
        await db.Specialties.AddRangeAsync(specialties);
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Specialties] ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Specialties] OFF", ct);
            await tx.CommitAsync(ct);
        }
        db.ChangeTracker.Clear();

        var hospitals = new[]
        {
            new Hospital { Id = 1, Name = "Nirmal Government Hospital", Address = "Main Road, Nirmal Town, Nirmal", Area = "Nirmal Town", Phone = "08734-231234", Type = HospitalType.Government, BedCount = 150, Latitude = 19.0969, Longitude = 78.3441, IsEmergency = true, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new Hospital { Id = 2, Name = "Sri Sai Multi-Specialty Hospital", Address = "Adilabad Road, Nirmal, Nirmal", Area = "Nirmal", Phone = "08734-245678", Type = HospitalType.MultiSpecialty, BedCount = 50, Latitude = 19.0980, Longitude = 78.3460, IsEmergency = true, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new Hospital { Id = 3, Name = "Lakshmi Nursing Home", Address = "Bus Stand Road, Nirmal, Nirmal", Area = "Nirmal", Phone = "08734-234567", Type = HospitalType.NursingHome, BedCount = 30, Latitude = 19.0975, Longitude = 78.3450, IsEmergency = false, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new Hospital { Id = 4, Name = "Bhainsa Area Hospital", Address = "Main Road, Bhainsa, Bhainsa", Area = "Bhainsa", Phone = "08752-223457", Type = HospitalType.Government, BedCount = 75, Latitude = 19.3123, Longitude = 77.8765, IsEmergency = true, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new Hospital { Id = 5, Name = "Nirmal Urban Health Centre", Address = "Town Centre, Nirmal", Area = "Nirmal", Phone = "08734-230001", Type = HospitalType.Government, BedCount = 20, Latitude = 19.0970, Longitude = 78.3435, IsEmergency = true, IsActive = true, CreatedAtUtc = DateTime.UtcNow }
        };
        await db.Hospitals.AddRangeAsync(hospitals);
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Hospitals] ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Hospitals] OFF", ct);
            await tx.CommitAsync(ct);
        }
        db.ChangeTracker.Clear();

        var hospitalSpecialties = new[]
        {
            new HospitalSpecialty { HospitalId = 1, SpecialtyId = 1 }, new HospitalSpecialty { HospitalId = 1, SpecialtyId = 2 }, new HospitalSpecialty { HospitalId = 1, SpecialtyId = 3 },
            new HospitalSpecialty { HospitalId = 2, SpecialtyId = 1 }, new HospitalSpecialty { HospitalId = 2, SpecialtyId = 4 }, new HospitalSpecialty { HospitalId = 2, SpecialtyId = 5 },
            new HospitalSpecialty { HospitalId = 3, SpecialtyId = 6 }, new HospitalSpecialty { HospitalId = 3, SpecialtyId = 3 }, new HospitalSpecialty { HospitalId = 3, SpecialtyId = 1 },
            new HospitalSpecialty { HospitalId = 4, SpecialtyId = 1 }, new HospitalSpecialty { HospitalId = 4, SpecialtyId = 2 }, new HospitalSpecialty { HospitalId = 4, SpecialtyId = 3 },
            new HospitalSpecialty { HospitalId = 5, SpecialtyId = 1 }, new HospitalSpecialty { HospitalId = 5, SpecialtyId = 3 }
        };
        await db.HospitalSpecialties.AddRangeAsync(hospitalSpecialties);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();

        var doctorNames = new[] { "Dr. Rajesh Kumar", "Dr. Sunita Reddy", "Dr. Mohan Singh", "Dr. Lakshmi Devi", "Dr. Suresh Babu", "Dr. Anitha Rao", "Dr. Venkat Rao", "Dr. Padma Sharma", "Dr. Ramesh Gupta", "Dr. Kavitha Nair", "Dr. Srinivas Reddy", "Dr. Geeta Singh", "Dr. Prakash Kumar", "Dr. Meena Devi", "Dr. Arun Sharma", "Dr. Priya Nair", "Dr. Vijay Kumar", "Dr. Swathi Reddy", "Dr. Mahesh Babu", "Dr. Divya Rao", "Dr. Karthik Singh", "Dr. Sandhya Devi", "Dr. Ravi Kumar", "Dr. Uma Reddy", "Dr. Sridhar Rao" };
        var doctors = new List<Doctor>();
        int docId = 1;
        for (int h = 1; h <= 5; h++)
        {
            for (int d = 0; d < 5; d++)
            {
                var name = doctorNames[(h - 1) * 5 + d];
                doctors.Add(new Doctor
                {
                    Id = docId++,
                    HospitalId = h,
                    FullName = name,
                    Qualification = "MBBS, MD",
                    Phone = "08734-" + (230000 + docId),
                    SupportsInPerson = true,
                    SupportsVideo = docId % 3 == 0,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }
        await db.Doctors.AddRangeAsync(doctors);
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Doctors] ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Doctors] OFF", ct);
            await tx.CommitAsync(ct);
        }
        db.ChangeTracker.Clear();

        var doctorSpecialties = new List<DoctorSpecialty>();
        for (int docId_ = 1; docId_ <= 25; docId_++)
        {
            var specId = (docId_ % 7) + 1;
            if (specId > 7) specId = 1;
            doctorSpecialties.Add(new DoctorSpecialty { DoctorId = docId_, SpecialtyId = specId });
        }
        await db.DoctorSpecialties.AddRangeAsync(doctorSpecialties);

        var slotTemplates = new List<SlotTemplate>();
        for (int d = 1; d <= 25; d++)
        {
            slotTemplates.Add(new SlotTemplate { DoctorId = d, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), ConsultationType = ConsultationType.InPerson, DurationMinutes = 15, IsActive = true, CreatedAtUtc = DateTime.UtcNow });
            slotTemplates.Add(new SlotTemplate { DoctorId = d, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(17, 0), ConsultationType = ConsultationType.InPerson, DurationMinutes = 15, IsActive = true, CreatedAtUtc = DateTime.UtcNow });
            if (d % 3 == 0)
                slotTemplates.Add(new SlotTemplate { DoctorId = d, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0), ConsultationType = ConsultationType.Video, DurationMinutes = 20, IsActive = true, CreatedAtUtc = DateTime.UtcNow });
        }
        await db.SlotTemplates.AddRangeAsync(slotTemplates);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();

        var superAdminEmail = "admin@nirmalhealth.in";
        if (!await db.Users.AnyAsync(u => u.Email == superAdminEmail, ct))
        {
            var superAdmin = new User
            {
                Email = superAdminEmail,
                Phone = "9999999999",
                PasswordHash = HashPassword("Admin@123"),
                FullName = "Super Admin",
                PreferredLanguage = "en",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(superAdmin);
            await db.SaveChangesAsync(ct);
            db.UserRoles.Add(new UserRole { UserId = superAdmin.Id, RoleId = 1 });
            await db.SaveChangesAsync(ct);
        }

        // Seed one patient and one hospital admin for testing
        var patientEmail = "patient@nirmalhealth.in";
        if (!await db.Users.AnyAsync(u => u.Email == patientEmail, ct))
        {
            var patient = new User
            {
                Email = patientEmail,
                Phone = "9876543210",
                PasswordHash = HashPassword("Patient@123"),
                FullName = "Test Patient",
                PreferredLanguage = "en",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(patient);
            await db.SaveChangesAsync(ct);
            db.UserRoles.Add(new UserRole { UserId = patient.Id, RoleId = 3 });
            await db.SaveChangesAsync(ct);
        }

        var hospitalAdminEmail = "hospital@nirmalhealth.in";
        if (!await db.Users.AnyAsync(u => u.Email == hospitalAdminEmail, ct))
        {
            var hospitalAdmin = new User
            {
                Email = hospitalAdminEmail,
                Phone = "9876543211",
                PasswordHash = HashPassword("Hospital@123"),
                FullName = "Nirmal Govt Hospital Admin",
                PreferredLanguage = "en",
                HospitalId = 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Users.Add(hospitalAdmin);
            await db.SaveChangesAsync(ct);
            db.UserRoles.Add(new UserRole { UserId = hospitalAdmin.Id, RoleId = 2 });
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation("Seeding completed.");
    }
}
