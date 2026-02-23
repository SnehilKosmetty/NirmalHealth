using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Appointments;
using NirmalHealth.Api.Services;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService) => _appointmentService = appointmentService;

    [HttpGet("slots")]
    public async Task<ActionResult<IEnumerable<SlotDto>>> GetAvailableSlots([FromQuery] int doctorId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? consultationType, CancellationToken ct)
    {
        if (to < from || (to.DayNumber - from.DayNumber) > 30) return BadRequest("Invalid date range.");
        var list = await _appointmentService.GetAvailableSlotsAsync(doctorId, from, to, consultationType, ct);
        return Ok(list);
    }

    [Authorize]
    [HttpPost("book")]
    public async Task<ActionResult<AppointmentDto>> Book([FromBody] BookAppointmentRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var patientId)) return Unauthorized();
        var a = await _appointmentService.BookAsync(patientId, body.SlotId, body.ChiefComplaint, ct);
        if (a == null) return BadRequest("Slot not available.");
        return Ok(a);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> MyAppointments(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id)) return Unauthorized();
        var list = await _appointmentService.GetMyAppointmentsAsync(id, ct);
        return Ok(list);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var hospitalId = User.FindFirstValue("hospital_id");
        int? hid = int.TryParse(hospitalId, out var h) ? h : null;
        int? uid = int.TryParse(userId ?? "", out var u) ? u : null;
        var a = await _appointmentService.GetAppointmentByIdAsync(id, uid, role, hid, ct);
        if (a == null) return NotFound();
        return Ok(a);
    }
}

public class BookAppointmentRequest
{
    public int SlotId { get; set; }
    public string? ChiefComplaint { get; set; }
}
