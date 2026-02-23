using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NirmalHealth.Api.Models.Auth;
using NirmalHealth.Api.Services;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<LoginResultDto>> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _auth.RegisterAsync(dto, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _auth.LoginAsync(dto, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResultDto>> Refresh([FromBody] RefreshRequest body, CancellationToken ct)
    {
        var result = await _auth.RefreshTokenAsync(body.RefreshToken ?? "", ct);
        if (result == null) return Unauthorized();
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfoDto>> Me(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id)) return Unauthorized();
        var user = await _auth.GetUserInfoAsync(id, ct);
        if (user == null) return NotFound();
        return Ok(user);
    }
}

public class RefreshRequest { public string? RefreshToken { get; set; } }
