using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Models.Doctors;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Services;

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _db;

    public DoctorService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<DoctorListDto>> GetDoctorsByHospitalAsync(int hospitalId, CancellationToken ct = default)
    {
        var list = await _db.Doctors
            .AsNoTracking()
            .Include(d => d.Hospital)
            .Include(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty)
            .Where(d => d.HospitalId == hospitalId && d.IsActive)
            .OrderBy(d => d.FullName)
            .ToListAsync(ct);
        return list.Select(MapToListDto);
    }

    public async Task<DoctorDetailDto?> GetDoctorByIdAsync(int id, CancellationToken ct = default)
    {
        var d = await _db.Doctors
            .AsNoTracking()
            .Include(d => d.Hospital)
            .Include(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive, ct);
        if (d == null) return null;
        var dto = new DoctorDetailDto
        {
            Id = d.Id,
            HospitalId = d.HospitalId,
            HospitalName = d.Hospital.Name,
            FullName = d.FullName,
            Qualification = d.Qualification,
            RegistrationNumber = d.RegistrationNumber,
            Specialties = d.DoctorSpecialties.Select(ds => ds.Specialty.Name).ToArray(),
            SupportsInPerson = d.SupportsInPerson,
            SupportsVideo = d.SupportsVideo,
            Phone = d.Phone,
            Email = d.Email
        };
        return dto;
    }

    public async Task<IEnumerable<DoctorListDto>> GetDoctorsAsync(int? hospitalId, string? specialty, CancellationToken ct = default)
    {
        var q = _db.Doctors
            .AsNoTracking()
            .Include(d => d.Hospital)
            .Include(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty)
            .Where(d => d.IsActive);
        if (hospitalId.HasValue) q = q.Where(d => d.HospitalId == hospitalId.Value);
        if (!string.IsNullOrWhiteSpace(specialty))
            q = q.Where(d => d.DoctorSpecialties.Any(ds => ds.Specialty.Name == specialty.Trim()));
        var list = await q.OrderBy(d => d.FullName).ToListAsync(ct);
        return list.Select(MapToListDto);
    }

    private static DoctorListDto MapToListDto(Core.Entities.Doctor d) => new()
    {
        Id = d.Id,
        HospitalId = d.HospitalId,
        HospitalName = d.Hospital?.Name ?? "",
        FullName = d.FullName,
        Qualification = d.Qualification,
        Specialties = d.DoctorSpecialties?.Select(ds => ds.Specialty.Name).ToArray() ?? Array.Empty<string>(),
        SupportsInPerson = d.SupportsInPerson,
        SupportsVideo = d.SupportsVideo,
        Phone = d.Phone
    };
}
