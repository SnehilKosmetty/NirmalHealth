using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Models.Hospitals;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Services;

public class HospitalService : IHospitalService
{
    private readonly AppDbContext _db;

    public HospitalService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<HospitalListDto>> GetHospitalsAsync(HospitalFilterDto? filter, CancellationToken ct = default)
    {
        var q = _db.Hospitals
            .AsNoTracking()
            .Include(h => h.HospitalSpecialties).ThenInclude(hs => hs.Specialty)
            .Where(h => h.IsActive);

        if (!string.IsNullOrWhiteSpace(filter?.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(h => h.Name.ToLower().Contains(s) || (h.Address != null && h.Address.ToLower().Contains(s)) || (h.Area != null && h.Area.ToLower().Contains(s)));
        }
        if (!string.IsNullOrWhiteSpace(filter?.Specialty))
        {
            var spec = filter.Specialty.Trim();
            q = q.Where(h => h.HospitalSpecialties.Any(hs => hs.Specialty.Name == spec));
        }
        if (!string.IsNullOrWhiteSpace(filter?.Type) && Enum.TryParse<HospitalType>(filter.Type, true, out var type))
            q = q.Where(h => h.Type == type);

        var list = await q.OrderBy(h => h.Name).ToListAsync(ct);
        return list.Select(MapToListDto);
    }

    public async Task<HospitalDetailDto?> GetHospitalByIdAsync(int id, CancellationToken ct = default)
    {
        var h = await _db.Hospitals
            .AsNoTracking()
            .Include(h => h.HospitalSpecialties).ThenInclude(hs => hs.Specialty)
            .Include(h => h.Doctors).ThenInclude(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty)
            .FirstOrDefaultAsync(h => h.Id == id && h.IsActive, ct);
        if (h == null) return null;
        var dto = new HospitalDetailDto
        {
            Id = h.Id,
            Name = h.Name,
            Address = h.Address,
            Area = h.Area,
            Phone = h.Phone,
            Email = h.Email,
            Type = h.Type.ToString(),
            BedCount = h.BedCount,
            Latitude = h.Latitude,
            Longitude = h.Longitude,
            IsEmergency = h.IsEmergency,
            Specialties = h.HospitalSpecialties.Select(hs => hs.Specialty.Name).ToArray(),
            Doctors = h.Doctors.Where(d => d.IsActive).Select(d => new DoctorSummaryDto
            {
                Id = d.Id,
                FullName = d.FullName,
                Qualification = d.Qualification,
                Specialties = d.DoctorSpecialties.Select(ds => ds.Specialty.Name).ToArray(),
                SupportsInPerson = d.SupportsInPerson,
                SupportsVideo = d.SupportsVideo,
                Phone = d.Phone
            }).ToList()
        };
        return dto;
    }

    public async Task<IEnumerable<HospitalListDto>> GetNearestHospitalsAsync(double? lat, double? lon, int count = 10, bool emergencyOnly = false, CancellationToken ct = default)
    {
        var q = _db.Hospitals
            .AsNoTracking()
            .Include(h => h.HospitalSpecialties).ThenInclude(hs => hs.Specialty)
            .Where(h => h.IsActive && (!emergencyOnly || h.IsEmergency));

        var list = await q.ToListAsync(ct);
        var withDistance = list.Select(h =>
        {
            var dto = MapToListDto(h);
            if (lat.HasValue && lon.HasValue && h.Latitude.HasValue && h.Longitude.HasValue)
                dto.DistanceKm = HaversineKm(lat.Value, lon.Value, h.Latitude.Value, h.Longitude.Value);
            return dto;
        }).OrderBy(x => x.DistanceKm ?? double.MaxValue).Take(count).ToList();
        return withDistance;
    }

    private static HospitalListDto MapToListDto(Hospital h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        Address = h.Address,
        Area = h.Area,
        Phone = h.Phone,
        Type = h.Type.ToString(),
        BedCount = h.BedCount,
        Latitude = h.Latitude,
        Longitude = h.Longitude,
        IsEmergency = h.IsEmergency,
        Specialties = h.HospitalSpecialties?.Select(hs => hs.Specialty.Name).ToArray() ?? Array.Empty<string>()
    };

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
