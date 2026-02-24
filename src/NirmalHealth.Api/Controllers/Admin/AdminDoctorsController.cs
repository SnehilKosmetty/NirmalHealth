using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Models.Doctors;
using NirmalHealth.Api.Services;
using NirmalHealth.Core.Constants;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.HospitalAdmin)]
public class AdminDoctorsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IDoctorService _doctorService;

    public AdminDoctorsController(AppDbContext db, IAuditService audit, IDoctorService doctorService)
    {
        _db = db;
        _audit = audit;
        _doctorService = doctorService;
    }

    private static int? GetCurrentHospitalId(ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(ClaimTypes.Role);
        if (role == RoleNames.SuperAdmin) return null;
        var hid = user.FindFirstValue("hospital_id");
        return int.TryParse(hid ?? "", out var h) ? h : null;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorListDto>>> List([FromQuery] int? hospitalId, CancellationToken ct)
    {
        var myHospitalId = GetCurrentHospitalId(User);
        var effectiveHospitalId = myHospitalId ?? hospitalId;
        if (effectiveHospitalId == null && User.FindFirstValue(ClaimTypes.Role) == RoleNames.HospitalAdmin)
            return BadRequest("Hospital not assigned to your account.");
        var q = _db.Doctors.AsNoTracking().Include(d => d.Hospital).Include(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty).Where(d => d.IsActive);
        if (effectiveHospitalId.HasValue) q = q.Where(d => d.HospitalId == effectiveHospitalId.Value);
        var list = await q.OrderBy(d => d.FullName).ToListAsync(ct);
        return Ok(list.Select(d => new DoctorListDto
        {
            Id = d.Id,
            HospitalId = d.HospitalId,
            HospitalName = d.Hospital.Name,
            FullName = d.FullName,
            Qualification = d.Qualification,
            Specialties = d.DoctorSpecialties.Select(ds => ds.Specialty.Name).ToArray(),
            SupportsInPerson = d.SupportsInPerson,
            SupportsVideo = d.SupportsVideo,
            Phone = d.Phone
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorDetailDto>> Get(int id, CancellationToken ct)
    {
        var myHospitalId = GetCurrentHospitalId(User);
        var doctor = await _doctorService.GetDoctorByIdAsync(id, ct);
        if (doctor == null) return NotFound();
        if (myHospitalId.HasValue && doctor.HospitalId != myHospitalId.Value) return Forbid();
        return Ok(doctor);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorListDto>> Create([FromBody] CreateDoctorRequest req, CancellationToken ct)
    {
        var myHospitalId = GetCurrentHospitalId(User);
        var hospitalId = myHospitalId ?? req.HospitalId;
        if (!hospitalId.HasValue) return BadRequest("HospitalId required.");
        if (myHospitalId.HasValue && myHospitalId != hospitalId) return Forbid();
        var doctor = new Doctor
        {
            HospitalId = hospitalId.Value,
            FullName = req.FullName,
            Qualification = req.Qualification,
            RegistrationNumber = req.RegistrationNumber,
            Phone = req.Phone,
            Email = req.Email,
            SupportsInPerson = req.SupportsInPerson,
            SupportsVideo = req.SupportsVideo,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(ct);
        if (req.SpecialtyIds != null)
            foreach (var sid in req.SpecialtyIds)
                _db.DoctorSpecialties.Add(new DoctorSpecialty { DoctorId = doctor.Id, SpecialtyId = sid });
        await _db.SaveChangesAsync(ct);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _audit.LogAsync(int.TryParse(userId, out var uid) ? uid : null, "Doctor.Create", "Doctor", doctor.Id.ToString(), null, null, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(new DoctorListDto { Id = doctor.Id, HospitalId = doctor.HospitalId, FullName = doctor.FullName, SupportsInPerson = doctor.SupportsInPerson, SupportsVideo = doctor.SupportsVideo });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDoctorRequest req, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FindAsync(new object[] { id }, ct);
        if (doctor == null) return NotFound();
        var myHospitalId = GetCurrentHospitalId(User);
        if (myHospitalId.HasValue && doctor.HospitalId != myHospitalId) return Forbid();
        doctor.FullName = req.FullName ?? doctor.FullName;
        doctor.Qualification = req.Qualification ?? doctor.Qualification;
        doctor.RegistrationNumber = req.RegistrationNumber ?? doctor.RegistrationNumber;
        doctor.Phone = req.Phone ?? doctor.Phone;
        doctor.Email = req.Email ?? doctor.Email;
        doctor.SupportsInPerson = req.SupportsInPerson ?? doctor.SupportsInPerson;
        doctor.SupportsVideo = req.SupportsVideo ?? doctor.SupportsVideo;
        doctor.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _audit.LogAsync(int.TryParse(userId, out var uid) ? uid : null, "Doctor.Update", "Doctor", id.ToString(), null, null, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return NoContent();
    }
}

public class CreateDoctorRequest
{
    public int? HospitalId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool SupportsInPerson { get; set; } = true;
    public bool SupportsVideo { get; set; }
    public int[]? SpecialtyIds { get; set; }
}

public class UpdateDoctorRequest
{
    public string? FullName { get; set; }
    public string? Qualification { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool? SupportsInPerson { get; set; }
    public bool? SupportsVideo { get; set; }
}
