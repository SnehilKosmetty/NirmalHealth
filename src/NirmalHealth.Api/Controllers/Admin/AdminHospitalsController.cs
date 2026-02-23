using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Models.Hospitals;
using NirmalHealth.Api.Services;
using NirmalHealth.Core.Constants;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.HospitalAdmin)]
public class AdminHospitalsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public AdminHospitalsController(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int? GetCurrentHospitalId()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == RoleNames.SuperAdmin) return null;
        var hid = User.FindFirstValue("hospital_id");
        return int.TryParse(hid ?? "", out var h) ? h : null;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HospitalListDto>>> List(CancellationToken ct)
    {
        var hospitalId = GetCurrentHospitalId();
        var q = _db.Hospitals.AsNoTracking().Include(h => h.HospitalSpecialties).ThenInclude(hs => hs.Specialty).Where(h => h.IsActive);
        if (hospitalId.HasValue) q = q.Where(h => h.Id == hospitalId.Value);
        var list = await q.OrderBy(h => h.Name).ToListAsync(ct);
        var dtos = list.Select(h => new HospitalListDto
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
            Specialties = h.HospitalSpecialties.Select(hs => hs.Specialty.Name).ToArray()
        });
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HospitalDetailDto>> Get(int id, CancellationToken ct)
    {
        var hospitalId = GetCurrentHospitalId();
        if (hospitalId.HasValue && hospitalId != id) return Forbid();
        var h = await _db.Hospitals
            .Include(x => x.HospitalSpecialties).ThenInclude(hs => hs.Specialty)
            .Include(x => x.Doctors).ThenInclude(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (h == null) return NotFound();
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
        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<HospitalListDto>> Create([FromBody] CreateHospitalRequest req, CancellationToken ct)
    {
        var hospital = new Hospital
        {
            Name = req.Name,
            Address = req.Address,
            Area = req.Area,
            Phone = req.Phone,
            Email = req.Email,
            Type = Enum.TryParse<HospitalType>(req.Type, true, out var t) ? t : HospitalType.Private,
            BedCount = req.BedCount,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            IsEmergency = req.IsEmergency,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Hospitals.Add(hospital);
        await _db.SaveChangesAsync(ct);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _audit.LogAsync(int.TryParse(userId, out var uid) ? uid : null, "Hospital.Create", "Hospital", hospital.Id.ToString(), null, System.Text.Json.JsonSerializer.Serialize(hospital), HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(new HospitalListDto { Id = hospital.Id, Name = hospital.Name, Address = hospital.Address, Phone = hospital.Phone, Type = hospital.Type.ToString(), BedCount = hospital.BedCount, IsEmergency = hospital.IsEmergency });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateHospitalRequest req, CancellationToken ct)
    {
        var hospitalId = GetCurrentHospitalId();
        if (hospitalId.HasValue && hospitalId != id) return Forbid();
        var h = await _db.Hospitals.FindAsync(new object[] { id }, ct);
        if (h == null) return NotFound();
        var oldJson = System.Text.Json.JsonSerializer.Serialize(h);
        h.Name = req.Name ?? h.Name;
        h.Address = req.Address ?? h.Address;
        h.Area = req.Area ?? h.Area;
        h.Phone = req.Phone ?? h.Phone;
        h.Email = req.Email ?? h.Email;
        if (!string.IsNullOrEmpty(req.Type) && Enum.TryParse<HospitalType>(req.Type, true, out var t)) h.Type = t;
        if (req.BedCount.HasValue) h.BedCount = req.BedCount.Value;
        h.Latitude = req.Latitude ?? h.Latitude;
        h.Longitude = req.Longitude ?? h.Longitude;
        if (req.IsEmergency.HasValue) h.IsEmergency = req.IsEmergency.Value;
        h.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _audit.LogAsync(int.TryParse(userId, out var uid) ? uid : null, "Hospital.Update", "Hospital", id.ToString(), oldJson, System.Text.Json.JsonSerializer.Serialize(h), HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return NoContent();
    }
}

public class CreateHospitalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Type { get; set; } = "Private";
    public int BedCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsEmergency { get; set; } = true;
}

public class UpdateHospitalRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Area { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Type { get; set; }
    public int? BedCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool? IsEmergency { get; set; }
}
