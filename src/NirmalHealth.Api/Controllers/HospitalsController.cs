using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Hospitals;
using NirmalHealth.Api.Services;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HospitalsController : ControllerBase
{
    private readonly IHospitalService _hospitalService;

    public HospitalsController(IHospitalService hospitalService) => _hospitalService = hospitalService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HospitalListDto>>> GetHospitals([FromQuery] string? search, [FromQuery] string? specialty, [FromQuery] string? type, CancellationToken ct)
    {
        var filter = new HospitalFilterDto { Search = search, Specialty = specialty, Type = type };
        var list = await _hospitalService.GetHospitalsAsync(filter, ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HospitalDetailDto>> GetHospital(int id, CancellationToken ct)
    {
        var h = await _hospitalService.GetHospitalByIdAsync(id, ct);
        if (h == null) return NotFound();
        return Ok(h);
    }

    [HttpGet("nearest")]
    public async Task<ActionResult<IEnumerable<HospitalListDto>>> GetNearest([FromQuery] double? lat, [FromQuery] double? lon, [FromQuery] int count = 10, [FromQuery] bool emergencyOnly = false, CancellationToken ct = default)
    {
        var list = await _hospitalService.GetNearestHospitalsAsync(lat, lon, count, emergencyOnly, ct);
        return Ok(list);
    }
}
