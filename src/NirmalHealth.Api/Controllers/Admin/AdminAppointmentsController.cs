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
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> List([FromQuery] int? hospitalId, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var hidClaim = User.FindFirstValue("hospital_id");
        var claimHospitalId = int.TryParse(hidClaim ?? "", out var ch) ? ch : (int?)null;

        if (role == RoleNames.SuperAdmin)
        {
            // Super Admin: no hospitalId or 0 = all appointments; otherwise filter by hospital
            if (hospitalId is null or 0)
            {
                var all = await _appointmentService.GetAllAppointmentsAsync(date, ct);
                return Ok(all);
            }
            var list = await _appointmentService.GetHospitalAppointmentsAsync(hospitalId.Value, date, ct);
            return Ok(list);
        }

        // Hospital Admin: use claim hospital; only allow viewing their own hospital
        var effectiveHospitalId = claimHospitalId ?? (hospitalId > 0 ? hospitalId : null);
        if (!effectiveHospitalId.HasValue) return BadRequest("Hospital not assigned to your account.");
        if (hospitalId.HasValue && hospitalId.Value > 0 && hospitalId.Value != effectiveHospitalId.Value) return Forbid();
        var hospitalList = await _appointmentService.GetHospitalAppointmentsAsync(effectiveHospitalId.Value, date, ct);
        return Ok(hospitalList);
    }
}
