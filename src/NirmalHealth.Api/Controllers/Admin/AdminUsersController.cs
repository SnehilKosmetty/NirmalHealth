using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Services;
using NirmalHealth.Core.Constants;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;
using System.Security.Claims;

namespace NirmalHealth.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public AdminUsersController(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpPost("hospital-admin")]
    public async Task<ActionResult<object>> CreateHospitalAdmin([FromBody] CreateHospitalAdminRequest req, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email || u.Phone == req.Phone, ct))
            return BadRequest(new { message = "Email or phone already registered." });
        if (req.HospitalId <= 0 || !await _db.Hospitals.AnyAsync(h => h.Id == req.HospitalId, ct))
            return BadRequest(new { message = "Invalid HospitalId." });

        var user = new User
        {
            Email = req.Email.Trim().ToLowerInvariant(),
            Phone = req.Phone.Trim(),
            PasswordHash = AuthServiceHashPassword(req.Password),
            FullName = req.FullName.Trim(),
            PreferredLanguage = req.PreferredLanguage ?? "en",
            HospitalId = req.HospitalId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        var hospitalAdminRole = await _db.Roles.FirstAsync(r => r.Name == RoleNames.HospitalAdmin, ct);
        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = hospitalAdminRole.Id });
        await _db.SaveChangesAsync(ct);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _audit.LogAsync(int.TryParse(userId, out var uid) ? uid : null, "User.CreateHospitalAdmin", "User", user.Id.ToString(), null, null, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(new { id = user.Id, email = user.Email, hospitalId = user.HospitalId });
    }

    private static string AuthServiceHashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "NirmalHealth_Salt");
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public class CreateHospitalAdminRequest
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public string PreferredLanguage { get; set; } = "en";
}
