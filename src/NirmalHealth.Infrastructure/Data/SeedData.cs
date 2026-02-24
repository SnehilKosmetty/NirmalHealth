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
        if (await db.Roles.AnyAsync(ct))
        {
            await EnsureDemoUsersAsync(db, ct);
            return;
        }

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

        // 40 hospitals in Nirmal District, Telangana (govt + private; from nirmal.telangana.gov.in, Ayushman Bharat, district health)
        var now = DateTime.UtcNow;
        var hospitals = new List<Hospital>
        {
            // Government (12)
            new Hospital { Id = 1, Name = "Government Area Hospital Nirmal", Address = "Mini Tank Bund Road, Gajulpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-231234", Type = HospitalType.Government, BedCount = 150, Latitude = 19.0969, Longitude = 78.3441, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 2, Name = "Government Area Hospital Bhainsa", Address = "Main Road, Bhainsa, Nirmal District - 504101", Area = "Bhainsa", Phone = "08752-223456", Type = HospitalType.Government, BedCount = 100, Latitude = 19.3123, Longitude = 77.8765, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 3, Name = "District Hospital Nirmal", Address = "Nirmal Town, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230100", Type = HospitalType.Government, BedCount = 100, Latitude = 19.0970, Longitude = 78.3440, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 4, Name = "Mother & Child Health Hospital Nirmal", Address = "Gajulpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230101", Type = HospitalType.Government, BedCount = 50, Latitude = 19.0965, Longitude = 78.3438, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 5, Name = "Community Health Centre Khanapur", Address = "Khanapur, Nirmal District - 504102", Area = "Khanapur", Phone = "08734-252001", Type = HospitalType.Government, BedCount = 30, Latitude = 19.15, Longitude = 78.42, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 6, Name = "Community Health Centre Mudhole", Address = "Mudhole, Nirmal District - 504103", Area = "Mudhole", Phone = "08734-253001", Type = HospitalType.Government, BedCount = 30, Latitude = 19.22, Longitude = 78.38, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 7, Name = "Community Health Centre Narsapur", Address = "Narsapur, Nirmal District - 504104", Area = "Narsapur", Phone = "08734-254001", Type = HospitalType.Government, BedCount = 30, Latitude = 19.18, Longitude = 78.35, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 8, Name = "UPHC Ramnagar Nirmal", Address = "Ramnagar, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230102", Type = HospitalType.Government, BedCount = 20, Latitude = 19.0985, Longitude = 78.3455, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 9, Name = "PHC Sarangapur", Address = "Sarangapur Mandal, Nirmal District - 504105", Area = "Sarangapur", Phone = "08734-255001", Type = HospitalType.Government, BedCount = 15, Latitude = 19.25, Longitude = 78.40, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 10, Name = "PHC Laxmanchanda", Address = "Laxmanchanda Mandal, Nirmal District - 504107", Area = "Laxmanchanda", Phone = "08734-256001", Type = HospitalType.Government, BedCount = 15, Latitude = 19.12, Longitude = 78.30, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 11, Name = "PHC Dilawarpur", Address = "Dilawarpur, Nirmal District - 504108", Area = "Dilawarpur", Phone = "08734-257001", Type = HospitalType.Government, BedCount = 15, Latitude = 19.20, Longitude = 78.36, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 12, Name = "PHC Soan", Address = "Soan Mandal, Nirmal District - 504109", Area = "Soan", Phone = "08734-258001", Type = HospitalType.Government, BedCount = 15, Latitude = 19.28, Longitude = 77.92, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            // Private (28)
            new Hospital { Id = 13, Name = "Kanika Kidney Care", Address = "Adilabad Road, Opposite Tirumala Theatre, Budhwarpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-245678", Type = HospitalType.Private, BedCount = 30, Latitude = 19.0980, Longitude = 78.3460, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 14, Name = "Nirmal Neuro Hospital", Address = "Old Bus Stand Road, Nirmal HO - 504106", Area = "Nirmal", Phone = "08734-234567", Type = HospitalType.Private, BedCount = 25, Latitude = 19.0975, Longitude = 78.3450, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 15, Name = "Veda Children's Hospital", Address = "Doctor Lane Road, Opposite Sree Diagnostics, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230001", Type = HospitalType.Private, BedCount = 20, Latitude = 19.0970, Longitude = 78.3435, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 16, Name = "Sri Datta Sai Multi Speciality Hospital", Address = "Main Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230002", Type = HospitalType.MultiSpecialty, BedCount = 50, Latitude = 19.0972, Longitude = 78.3445, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 17, Name = "Surya Multi Speciality Hospital", Address = "Shastri Nagar, Ibrahim Bagh, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230003", Type = HospitalType.MultiSpecialty, BedCount = 40, Latitude = 19.0865, Longitude = 78.3494, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 18, Name = "Aradhana Nursing Home", Address = "Nirmal Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230004", Type = HospitalType.NursingHome, BedCount = 25, Latitude = 19.0978, Longitude = 78.3448, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 19, Name = "GK Hospitals Nirmal", Address = "Budhwarpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230005", Type = HospitalType.Private, BedCount = 35, Latitude = 19.0982, Longitude = 78.3462, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 20, Name = "Krishna Nursing Home", Address = "Near Bus Stand, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230006", Type = HospitalType.NursingHome, BedCount = 20, Latitude = 19.0973, Longitude = 78.3452, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 21, Name = "Prashanths GK Hospitals", Address = "Adilabad Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230007", Type = HospitalType.Private, BedCount = 30, Latitude = 19.0985, Longitude = 78.3465, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 22, Name = "Shri Aditya Multi Speciality Hospital", Address = "Gajulpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230008", Type = HospitalType.MultiSpecialty, BedCount = 45, Latitude = 19.0968, Longitude = 78.3442, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 23, Name = "Niharika Hospital", Address = "Doctor Lane, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230009", Type = HospitalType.Private, BedCount = 22, Latitude = 19.0971, Longitude = 78.3438, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 24, Name = "Mano Vaidya Neuro Psychiatry Hospital", Address = "Old Town, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230010", Type = HospitalType.Private, BedCount = 20, Latitude = 19.0968, Longitude = 78.3432, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 25, Name = "Aadhya Hospital", Address = "Main Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230011", Type = HospitalType.Private, BedCount = 28, Latitude = 19.0974, Longitude = 78.3440, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 26, Name = "Vasavi Skin Hospital", Address = "Budhwarpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230012", Type = HospitalType.Private, BedCount = 15, Latitude = 19.0980, Longitude = 78.3458, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 27, Name = "Tirumala Nursing Home", Address = "Doctor Lane Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230013", Type = HospitalType.NursingHome, BedCount = 18, Latitude = 19.0970, Longitude = 78.3436, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 28, Name = "Sri Datta Sai Multi Speciality Hospital Bhainsa", Address = "Pule Nagar, Sandesh Ginning, Bhainsa - 504101", Area = "Bhainsa", Phone = "08752-223100", Type = HospitalType.MultiSpecialty, BedCount = 40, Latitude = 19.3100, Longitude = 77.8750, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 29, Name = "Sai Krishna Hospital", Address = "Main Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230014", Type = HospitalType.Private, BedCount = 25, Latitude = 19.0975, Longitude = 78.3442, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 30, Name = "Care Hospital Nirmal", Address = "Adilabad Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230015", Type = HospitalType.Private, BedCount = 30, Latitude = 19.0982, Longitude = 78.3460, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 31, Name = "Swathi Nursing Home", Address = "Near RTC Bus Stand, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230016", Type = HospitalType.NursingHome, BedCount = 20, Latitude = 19.0972, Longitude = 78.3450, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 32, Name = "Prashanth Hospital Bhainsa", Address = "Main Road, Bhainsa - 504101", Area = "Bhainsa", Phone = "08752-223200", Type = HospitalType.Private, BedCount = 25, Latitude = 19.3110, Longitude = 77.8760, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 33, Name = "Lakshmi Hospital Nirmal", Address = "Gajulpet Road, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230017", Type = HospitalType.Private, BedCount = 22, Latitude = 19.0965, Longitude = 78.3438, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 34, Name = "Padma Hospital Laxmanchanda", Address = "Laxmanchanda Mandal, Nirmal District - 504107", Area = "Laxmanchanda", Phone = "08734-256100", Type = HospitalType.Private, BedCount = 20, Latitude = 19.1210, Longitude = 78.3010, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 35, Name = "Ramesh Nursing Home Sarangapur", Address = "Sarangapur, Nirmal District - 504105", Area = "Sarangapur", Phone = "08734-255100", Type = HospitalType.NursingHome, BedCount = 15, Latitude = 19.2510, Longitude = 78.4010, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 36, Name = "Devi Hospital Mudhole", Address = "Mudhole, Nirmal District - 504103", Area = "Mudhole", Phone = "08734-253100", Type = HospitalType.Private, BedCount = 20, Latitude = 19.2210, Longitude = 78.3810, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 37, Name = "Karthik Multi Speciality Hospital", Address = "Budhwarpet, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230018", Type = HospitalType.MultiSpecialty, BedCount = 35, Latitude = 19.0983, Longitude = 78.3463, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 38, Name = "Anitha Hospital Bhainsa", Address = "Bhainsa Town, Nirmal District - 504101", Area = "Bhainsa", Phone = "08752-223300", Type = HospitalType.Private, BedCount = 18, Latitude = 19.3090, Longitude = 77.8740, IsEmergency = true, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 39, Name = "Sree Diagnostics & Hospital", Address = "Doctor Lane, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230019", Type = HospitalType.Private, BedCount = 15, Latitude = 19.0970, Longitude = 78.3435, IsEmergency = false, IsActive = true, CreatedAtUtc = now },
            new Hospital { Id = 40, Name = "Sai Baba Hospital Nirmal", Address = "Ibrahim Bagh, Nirmal - 504106", Area = "Nirmal", Phone = "08734-230020", Type = HospitalType.Private, BedCount = 25, Latitude = 19.0870, Longitude = 78.3480, IsEmergency = true, IsActive = true, CreatedAtUtc = now }
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

        var hospitalSpecialties = new List<HospitalSpecialty>();
        for (int hId = 1; hId <= 40; hId++)
        {
            var s1 = ((hId - 1) % 7) + 1;
            var s2 = (hId % 7) + 1;
            var s3 = ((hId + 1) % 7) + 1;
            hospitalSpecialties.Add(new HospitalSpecialty { HospitalId = hId, SpecialtyId = s1 });
            if (s2 != s1) hospitalSpecialties.Add(new HospitalSpecialty { HospitalId = hId, SpecialtyId = s2 });
            if (s3 != s1 && s3 != s2) hospitalSpecialties.Add(new HospitalSpecialty { HospitalId = hId, SpecialtyId = s3 });
        }
        await db.HospitalSpecialties.AddRangeAsync(hospitalSpecialties);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();

        var doctorNames = new[]
        {
            "Dr. Rajesh Kumar", "Dr. Sunita Reddy", "Dr. Mohan Singh", "Dr. Lakshmi Devi", "Dr. Suresh Babu", "Dr. Anitha Rao", "Dr. Venkat Rao", "Dr. Padma Sharma", "Dr. Ramesh Gupta", "Dr. Kavitha Nair",
            "Dr. Srinivas Reddy", "Dr. Geeta Singh", "Dr. Prakash Kumar", "Dr. Meena Devi", "Dr. Arun Sharma", "Dr. Priya Nair", "Dr. Vijay Kumar", "Dr. Swathi Reddy", "Dr. Mahesh Babu", "Dr. Divya Rao",
            "Dr. Karthik Singh", "Dr. Sandhya Devi", "Dr. Ravi Kumar", "Dr. Uma Reddy", "Dr. Sridhar Rao", "Dr. Madhavi Latha", "Dr. Satish Reddy", "Dr. Padmini Nair", "Dr. Gopal Rao", "Dr. Indira Devi",
            "Dr. Chandrasekhar", "Dr. Vasundhara", "Dr. Nagaraju", "Dr. Sujatha", "Dr. Venugopal", "Dr. Lakshmi Kanth", "Dr. Radhika", "Dr. Siva Kumar", "Dr. Parvathi", "Dr. Raghunath",
            "Dr. Shobha Rani", "Dr. Balakrishna", "Dr. Kalpana", "Dr. Subba Rao", "Dr. Vijaya Lakshmi", "Dr. Narasimha", "Dr. Savithri", "Dr. Hanumantha Rao", "Dr. Sarojini", "Dr. Venkatesh",
            "Dr. Manjula", "Dr. Prabhakar", "Dr. Revathi", "Dr. Suryanarayana", "Dr. Padmavathi", "Dr. Ramakrishna", "Dr. Janaki", "Dr. Mohan Rao", "Dr. Lalitha", "Dr. Satyanarayana",
            "Dr. Kamala", "Dr. Venkata Rao", "Dr. Sumathi", "Dr. Krishna Murthy", "Dr. Varalakshmi", "Dr. Gopala Krishna", "Dr. Hemalatha", "Dr. Rama Rao", "Dr. Susheela", "Dr. Adinarayana",
            "Dr. Bhagya Lakshmi", "Dr. Samba Siva Rao", "Dr. Annapurna", "Dr. Tirupathi", "Dr. Saraswathi", "Dr. Nagesh", "Dr. Vijaya", "Dr. Raghava", "Dr. Kamakshi", "Dr. Prasada Rao"
        };
        var doctors = new List<Doctor>();
        int docId = 1;
        for (int h = 1; h <= 40; h++)
        {
            for (int d = 0; d < 2; d++)
            {
                var name = doctorNames[(docId - 1) % doctorNames.Length];
                doctors.Add(new Doctor
                {
                    Id = docId,
                    HospitalId = h,
                    FullName = name,
                    Qualification = "MBBS, MD",
                    Phone = "08734-" + (230000 + docId),
                    SupportsInPerson = true,
                    SupportsVideo = docId % 3 == 0,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
                docId++;
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
        for (int docId_ = 1; docId_ <= 80; docId_++)
        {
            var specId = (docId_ % 7) + 1;
            doctorSpecialties.Add(new DoctorSpecialty { DoctorId = docId_, SpecialtyId = specId });
        }
        await db.DoctorSpecialties.AddRangeAsync(doctorSpecialties);

        var slotTemplates = new List<SlotTemplate>();
        for (int d = 1; d <= 80; d++)
        {
            slotTemplates.Add(new SlotTemplate { DoctorId = d, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), ConsultationType = ConsultationType.InPerson, DurationMinutes = 15, IsActive = true, CreatedAtUtc = DateTime.UtcNow });
            slotTemplates.Add(new SlotTemplate { DoctorId = d, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(17, 0), ConsultationType = ConsultationType.InPerson, DurationMinutes = 15, IsActive = true, CreatedAtUtc = DateTime.UtcNow });
            if (d % 3 == 0)
                slotTemplates.Add(new SlotTemplate { DoctorId = d, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0), ConsultationType = ConsultationType.Video, DurationMinutes = 20, IsActive = true, CreatedAtUtc = DateTime.UtcNow });
        }
        await db.SlotTemplates.AddRangeAsync(slotTemplates);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();

        await EnsureDemoUsersAsync(db, ct);

        logger.LogInformation("Seeding completed.");
    }

    /// <summary>Ensures Super Admin, one Patient, and one Hospital Admin exist (creates if missing). Runs on every startup when DB already has data.</summary>
    private static async Task EnsureDemoUsersAsync(AppDbContext db, CancellationToken ct)
    {
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

        var patientEmail = "patient@nirmalhealth.in";
        if (!await db.Users.AnyAsync(u => u.Email == patientEmail, ct))
        {
            var patient = new User
            {
                Email = patientEmail,
                Phone = "9999999999",
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
                Phone = "9999999999",
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

        await EnsureDemoAppointmentsAsync(db, ct);
    }

    /// <summary>Ensures a few demo appointments exist so Super Admin / Hospital Admin see data. Runs after EnsureDemoUsersAsync.</summary>
    private static async Task EnsureDemoAppointmentsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Appointments.AnyAsync(ct))
            return;

        var patient = await db.Users.FirstOrDefaultAsync(u => u.Email == "patient@nirmalhealth.in", ct);
        if (patient == null)
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var tomorrow = today.AddDays(1);

        var slotsToAdd = new List<AppointmentSlot>
        {
            new() { DoctorId = 1, Date = today, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 15), ConsultationType = ConsultationType.InPerson, IsBooked = false, CreatedAtUtc = now },
            new() { DoctorId = 1, Date = today, StartTime = new TimeOnly(9, 15), EndTime = new TimeOnly(9, 30), ConsultationType = ConsultationType.InPerson, IsBooked = false, CreatedAtUtc = now },
            new() { DoctorId = 1, Date = today, StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(9, 45), ConsultationType = ConsultationType.InPerson, IsBooked = false, CreatedAtUtc = now },
            new() { DoctorId = 1, Date = tomorrow, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 15), ConsultationType = ConsultationType.InPerson, IsBooked = false, CreatedAtUtc = now },
            new() { DoctorId = 2, Date = today, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(14, 15), ConsultationType = ConsultationType.InPerson, IsBooked = false, CreatedAtUtc = now },
            new() { DoctorId = 2, Date = tomorrow, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(14, 15), ConsultationType = ConsultationType.InPerson, IsBooked = false, CreatedAtUtc = now },
        };
        await db.AppointmentSlots.AddRangeAsync(slotsToAdd, ct);
        await db.SaveChangesAsync(ct);

        var bookedSlots = slotsToAdd.Take(3).ToList();
        foreach (var slot in bookedSlots)
            slot.IsBooked = true;

        var appointments = new List<Appointment>
        {
            new() { PatientId = patient.Id, SlotId = bookedSlots[0].Id, ChiefComplaint = "Fever and cold", Status = AppointmentStatus.Scheduled, CreatedAtUtc = now },
            new() { PatientId = patient.Id, SlotId = bookedSlots[1].Id, ChiefComplaint = "Follow-up", Status = AppointmentStatus.Scheduled, CreatedAtUtc = now },
            new() { PatientId = patient.Id, SlotId = bookedSlots[2].Id, ChiefComplaint = "General checkup", Status = AppointmentStatus.Scheduled, CreatedAtUtc = now },
        };
        await db.Appointments.AddRangeAsync(appointments, ct);
        await db.SaveChangesAsync(ct);
    }
}
