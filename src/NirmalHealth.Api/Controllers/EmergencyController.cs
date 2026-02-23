using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Hospitals;
using NirmalHealth.Api.Services;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmergencyController : ControllerBase
{
    private readonly IHospitalService _hospitalService;

    public EmergencyController(IHospitalService hospitalService) => _hospitalService = hospitalService;

    [HttpGet("contacts")]
    public IActionResult GetContacts()
    {
        var contacts = new[]
        {
            new { name = "Ambulance", number = "108", description = "Free ambulance service" },
            new { name = "Police", number = "100", description = "Police emergency" },
            new { name = "Fire", number = "101", description = "Fire emergency" },
            new { name = "Women Helpline", number = "181", description = "Women safety helpline" },
            new { name = "Child Helpline", number = "1098", description = "Child protection helpline" }
        };
        return Ok(contacts);
    }

    [HttpGet("nearest-hospitals")]
    public async Task<ActionResult<IEnumerable<HospitalListDto>>> GetNearestHospitals([FromQuery] double? lat, [FromQuery] double? lon, [FromQuery] int count = 5, CancellationToken ct = default)
    {
        var list = await _hospitalService.GetNearestHospitalsAsync(lat, lon, count, emergencyOnly: true, ct);
        return Ok(list);
    }
}
