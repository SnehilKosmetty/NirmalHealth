using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<StatsDto>> GetStats(CancellationToken ct)
    {
        var hospitals = await _db.Hospitals.CountAsync(h => h.IsActive, ct);
        var doctors = await _db.Doctors.CountAsync(d => d.IsActive, ct);
        return Ok(new StatsDto { HospitalsCount = hospitals, DoctorsCount = doctors });
    }
}

public class StatsDto
{
    public int HospitalsCount { get; set; }
    public int DoctorsCount { get; set; }
}
