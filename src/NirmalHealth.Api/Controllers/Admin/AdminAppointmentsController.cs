using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Appointments;
using NirmalHealth.Api.Services;
using NirmalHealth.Core.Constants;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.HospitalAdmin)]
public class AdminAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AdminAppointmentsController(IAppointmentService appointmentService) => _appointmentService = appointmentService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> List([FromQuery] int hospitalId, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var hidClaim = User.FindFirstValue("hospital_id");
        var effectiveHospitalId = role == RoleNames.SuperAdmin ? hospitalId : (int.TryParse(hidClaim ?? "", out var h) ? h : (int?)null);
        if (!effectiveHospitalId.HasValue) return BadRequest("hospitalId required.");
        if (role == RoleNames.HospitalAdmin && effectiveHospitalId != hospitalId) return Forbid();
        var list = await _appointmentService.GetHospitalAppointmentsAsync(effectiveHospitalId.Value, date, ct);
        return Ok(list);
    }
}
