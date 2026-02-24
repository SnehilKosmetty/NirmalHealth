using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NirmalHealth.Api.Models.Auth;
using NirmalHealth.Core.Constants;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>One-way hash of Aadhaar. Full number is never stored anywhere.</summary>
    private string HashAadhaar(string digits12)
    {
        var salt = _config["Secrets:AadhaarSalt"] ?? _config["Aadhaar:Salt"] ?? throw new InvalidOperationException("Secrets:AadhaarSalt or Aadhaar:Salt must be set for Aadhaar verification.");
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(salt + digits12);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public async Task<LoginResultDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email || u.Phone == dto.Phone, ct))
            throw new InvalidOperationException("Email or phone already registered.");

        var digits = string.IsNullOrWhiteSpace(dto.AadhaarNumber) ? null : new string(dto.AadhaarNumber.Where(char.IsDigit).ToArray());
        if (digits == null || digits.Length != 12)
            throw new InvalidOperationException("Aadhaar verification is required to create your profile. Please enter your 12-digit Aadhaar number.");

        // Store only one-way hash; no part of Aadhaar is stored in plain text.
        var aadhaarHash = HashAadhaar(digits);

        var user = new User
        {
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone.Trim(),
            PasswordHash = HashPassword(dto.Password),
            FullName = dto.FullName.Trim(),
            AadhaarMasked = null,
            AadhaarHash = aadhaarHash,
            AadhaarVerified = true,
            PreferredLanguage = dto.PreferredLanguage ?? "en",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var patientRole = await _db.Roles.FirstAsync(r => r.Name == RoleNames.Patient, ct);
        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = patientRole.Id });
        await _db.SaveChangesAsync(ct);

        return await BuildLoginResultAsync(user.Id, ct);
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var key = dto.EmailOrPhone.Trim().ToLowerInvariant();
        var isEmail = key.Contains("@");
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.IsActive && (isEmail ? u.Email == key : u.Phone == dto.EmailOrPhone.Trim()), ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await BuildLoginResultAsync(user.Id, ct);
    }

    public async Task<LoginResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // In production store refresh tokens in DB and validate. Simplified here.
        var principal = ValidateToken(refreshToken, isRefresh: true);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId)) return null;
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user == null || !user.IsActive) return null;
        return await BuildLoginResultAsync(userId, ct);
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        if (user == null) return null;
        return ToUserInfo(user);
    }

    public async Task<UserInfoDto?> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        if (user == null) return null;

        // If Aadhaar is not yet verified, only allow submitting Aadhaar (to verify). No other profile fields can be updated until verified.
        if (!user.AadhaarVerified)
        {
            var digits = string.IsNullOrWhiteSpace(dto.AadhaarNumber) ? null : new string(dto.AadhaarNumber.Where(char.IsDigit).ToArray());
            if (digits == null || digits.Length != 12)
                throw new InvalidOperationException("Please verify your Aadhaar with a 12-digit number to create and update your profile.");
            user.AadhaarHash = HashAadhaar(digits);
            user.AadhaarMasked = null;
            user.AadhaarVerified = true;
            user.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return ToUserInfo(user);
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName.Trim();

        if (dto.Email != null)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (email != user.Email && await _db.Users.AnyAsync(u => u.Id != userId && u.Email == email, ct))
                throw new InvalidOperationException("Email already in use.");
            user.Email = email;
        }

        if (dto.Phone != null)
        {
            var phone = dto.Phone.Trim();
            if (phone != user.Phone && await _db.Users.AnyAsync(u => u.Id != userId && u.Phone == phone, ct))
                throw new InvalidOperationException("Phone already in use.");
            user.Phone = phone;
        }

        if (dto.PreferredLanguage != null)
            user.PreferredLanguage = dto.PreferredLanguage.Trim().Length > 0 ? dto.PreferredLanguage.Trim() : "en";

        if (!string.IsNullOrWhiteSpace(dto.AadhaarNumber))
        {
            var digits = new string(dto.AadhaarNumber.Where(char.IsDigit).ToArray());
            if (digits.Length == 12)
            {
                user.AadhaarHash = HashAadhaar(digits);
                user.AadhaarMasked = null;
                user.AadhaarVerified = true;
            }
        }

        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ToUserInfo(user);
    }

    private async Task<LoginResultDto> BuildLoginResultAsync(int userId, CancellationToken ct)
    {
        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstAsync(u => u.Id == userId, ct);
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = GenerateRefreshToken(user);
        var expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes());
        return new LoginResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expires,
            User = ToUserInfo(user)
        };
    }

    private string GenerateAccessToken(User user, string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };
        foreach (var role in roles)
            claims.Add(new(ClaimTypes.Role, role));
        if (user.HospitalId.HasValue)
            claims.Add(new("hospital_id", user.HospitalId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token, bool isRefresh)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
            }, out _);
            return principal;
        }
        catch { return null; }
    }

    /// <summary>Never expose any part of Aadhaar to client. Only verified status.</summary>
    private static UserInfoDto ToUserInfo(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Phone = user.Phone,
        FullName = user.FullName,
        PreferredLanguage = user.PreferredLanguage,
        AadhaarMasked = user.AadhaarVerified ? "Verified" : null,
        AadhaarVerified = user.AadhaarVerified,
        Roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray(),
        HospitalId = user.HospitalId
    };

    private int GetAccessTokenExpiryMinutes() => int.TryParse(_config["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 60;
    private int GetRefreshTokenExpiryDays() => int.TryParse(_config["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 7;

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "NirmalHealth_Salt");
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var computed = HashPassword(password);
        return string.Equals(computed, hash, StringComparison.Ordinal);
    }
}
