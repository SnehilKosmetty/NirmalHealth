using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Symptoms;
using NirmalHealth.Api.Services;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SymptomsController : ControllerBase
{
    private readonly ISymptomAnalysisService _symptomService;

    public SymptomsController(ISymptomAnalysisService symptomService) => _symptomService = symptomService;

    [HttpPost("analyze")]
    public async Task<ActionResult<SymptomAnalysisResultDto>> Analyze([FromBody] SymptomAnalysisRequestDto dto, CancellationToken ct)
    {
        int? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var uid))
            userId = uid;
        var result = await _symptomService.AnalyzeAsync(dto, userId, ct);
        return Ok(result);
    }
}
