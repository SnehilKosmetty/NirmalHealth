using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NirmalHealth.Core.Constants;
using NirmalHealth.Infrastructure.Data;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.HospitalAdmin)]
public class AdminStatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminStatsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<AdminStatsDto>> Get(CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var hidClaim = User.FindFirstValue("hospital_id");
        int? hospitalId = int.TryParse(hidClaim ?? "", out var h) ? h : null;

        var hospitalsQuery = _db.Hospitals.Where(x => x.IsActive);
        var doctorsQuery = _db.Doctors.Where(d => d.IsActive);
        var appointmentsQuery = _db.Appointments.AsQueryable();

        if (role == RoleNames.HospitalAdmin && hospitalId.HasValue)
        {
            hospitalsQuery = hospitalsQuery.Where(x => x.Id == hospitalId.Value);
            doctorsQuery = doctorsQuery.Where(d => d.HospitalId == hospitalId.Value);
            appointmentsQuery = _db.Appointments
                .Where(a => a.Slot.Doctor.HospitalId == hospitalId.Value);
        }

        var hospitalsCount = await hospitalsQuery.CountAsync(ct);
        var doctorsCount = await doctorsQuery.CountAsync(ct);
        var appointmentsCount = await appointmentsQuery.CountAsync(ct);

        return Ok(new AdminStatsDto
        {
            HospitalsCount = hospitalsCount,
            DoctorsCount = doctorsCount,
            AppointmentsCount = appointmentsCount
        });
    }
}

public class AdminStatsDto
{
    public int HospitalsCount { get; set; }
    public int DoctorsCount { get; set; }
    public int AppointmentsCount { get; set; }
}
