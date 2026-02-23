namespace NirmalHealth.Api.Models.Auth;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    /// <summary>12-digit Aadhaar. Sent to eKYC provider; we store only masked + verified flag.</summary>
    public string? AadhaarNumber { get; set; }
    public string PreferredLanguage { get; set; } = "en";
}
