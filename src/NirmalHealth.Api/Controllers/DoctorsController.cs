using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Doctors;
using NirmalHealth.Api.Services;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService) => _doctorService = doctorService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorListDto>>> GetDoctors([FromQuery] int? hospitalId, [FromQuery] string? specialty, CancellationToken ct)
    {
        var list = await _doctorService.GetDoctorsAsync(hospitalId, specialty, ct);
        return Ok(list);
    }

    [HttpGet("hospital/{hospitalId:int}")]
    public async Task<ActionResult<IEnumerable<DoctorListDto>>> GetByHospital(int hospitalId, CancellationToken ct)
    {
        var list = await _doctorService.GetDoctorsByHospitalAsync(hospitalId, ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorDetailDto>> GetDoctor(int id, CancellationToken ct)
    {
        var d = await _doctorService.GetDoctorByIdAsync(id, ct);
        if (d == null) return NotFound();
        return Ok(d);
    }
}
